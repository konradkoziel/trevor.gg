using System.Collections.Concurrent;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;

namespace trevor.Infrastructure.Storage;

// Table clients for the Function App's storage account, each table created on first use,
// so a storage problem breaks only the features that need tables, not the whole app
public class StorageTables(IOptions<StorageOptions> options)
{
    private readonly ConcurrentDictionary<string, TableClient> _tables = new();

    public async Task<TableClient> GetAsync(string tableName, CancellationToken ct = default)
    {
        if (_tables.TryGetValue(tableName, out var ready))
            return ready;

        var connectionString = options.Value.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Table Storage needs AzureWebJobsStorage as a connection string");

        var table = new TableClient(connectionString, tableName);
        await table.CreateIfNotExistsAsync(ct);
        return _tables.GetOrAdd(tableName, table);
    }
}
