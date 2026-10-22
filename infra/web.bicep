// Azure resources of the website, deployed into resource group "trevor-web" by .github/workflows/web.yml.
// The bot (Function App "Trevor") lives in its own resource group and is not touched by this file.

@description('Region of the API, its storage and monitoring')
param location string = resourceGroup().location

@description('Static Web Apps exist only in a few regions; the site itself is served from a global CDN anyway')
param staticSiteLocation string = 'westeurope'

@description('Discord application id (the bot\'s), for "Log in with Discord"; empty leaves login off')
param discordAppId string = ''

@description('OAuth2 client secret of that Discord application')
@secure()
param discordClientSecret string = ''

@description('Id of Trevor\'s Discord server: only its members may log in; empty lets any Discord user in')
param discordGuildId string = ''

@description('Key the bot sends to /api/bot/... (the bot gets the same secret); empty turns those endpoints off')
@secure()
param trevorWebKey string = ''

@description('Riot API key, for linking League accounts and stats; empty leaves those off')
@secure()
param riotApiKey string = ''

@description('How many recent games the stats show and storage keeps per player (GitHub variable MATCH_HISTORY_GAMES); older ones are deleted')
@minValue(1)
@maxValue(100)
param matchHistoryGames int = 20

// Storage account and Function App names are global, so they get a suffix that is stable per resource group
var suffix = uniqueString(resourceGroup().id)
var storageName = 'trevorweb${suffix}'
var apiName = 'trevor-web-api-${suffix}'

// Flex Consumption apps get <name>.azurewebsites.net; Discord needs this exact URL under OAuth2 → Redirects
var discordRedirectUri = 'https://${apiName}.azurewebsites.net/api/auth/discord/callback'

// Blob container the Flex Consumption plan deploys the API package into
var deploymentContainer = 'api-package'

// Flex Consumption: pay per execution with a monthly free grant; the cap keeps a traffic spike from becoming a bill
var maximumInstanceCount = 40
var instanceMemoryMB = 2048

resource site 'Microsoft.Web/staticSites@2024-04-01' = {
  name: 'trevor-web'
  location: staticSiteLocation
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
}

resource packageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: deploymentContainer
}

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'trevor-web-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource insights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'trevor-web-api'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logs.id
  }
}

resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: 'trevor-web-plan'
  location: location
  kind: 'functionapp'
  sku: {
    tier: 'FlexConsumption'
    name: 'FC1'
  }
  properties: {
    reserved: true
  }
}

var storageConnection = 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

resource api 'Microsoft.Web/sites@2024-04-01' = {
  name: apiName
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      minTlsVersion: '1.2'
      cors: {
        // Only the website may call the API from a browser
        allowedOrigins: [
          'https://${site.properties.defaultHostname}'
        ]
      }
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageConnection
        }
        {
          name: 'DEPLOYMENT_STORAGE_CONNECTION_STRING'
          value: storageConnection
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: insights.properties.ConnectionString
        }
        {
          name: 'DISCORD_APP_ID'
          value: discordAppId
        }
        {
          name: 'DISCORD_CLIENT_SECRET'
          value: discordClientSecret
        }
        {
          name: 'DISCORD_REDIRECT_URI'
          value: discordRedirectUri
        }
        {
          name: 'DISCORD_GUILD_ID'
          value: discordGuildId
        }
        {
          name: 'TREVOR_WEB_KEY'
          value: trevorWebKey
        }
        {
          name: 'RIOT_API_KEY'
          value: riotApiKey
        }
        {
          name: 'MATCH_HISTORY_GAMES'
          value: string(matchHistoryGames)
        }
        {
          name: 'SITE_URL'
          value: 'https://${site.properties.defaultHostname}'
        }
      ]
    }
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storage.properties.primaryEndpoints.blob}${deploymentContainer}'
          authentication: {
            type: 'StorageAccountConnectionString'
            storageAccountConnectionStringName: 'DEPLOYMENT_STORAGE_CONNECTION_STRING'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
    }
  }
  dependsOn: [
    packageContainer
  ]
}

output staticSiteName string = site.name
output siteUrl string = 'https://${site.properties.defaultHostname}'
output apiName string = api.name
output apiUrl string = 'https://${api.properties.defaultHostName}'
output discordRedirectUri string = discordRedirectUri
