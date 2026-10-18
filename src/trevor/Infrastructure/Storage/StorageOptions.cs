namespace trevor.Infrastructure.Storage;

public class StorageOptions
{
    // The Function App's own storage account (AzureWebJobsStorage), as a connection string
    public string? ConnectionString { get; set; }
}
