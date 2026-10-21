using System.Security.Cryptography;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Web.Api.Infrastructure.Sessions;

// Keeps the key in the API's own storage account, generated on first use, so nobody has to create
// or rotate a secret by hand. Deleting the blob logs everybody out.
public class BlobSigningKeyStore(BlobServiceClient blobs) : ISigningKeyStore
{
    private const string ContainerName = "sessions";
    private const string BlobName = "signing-key";

    // HMAC-SHA256 key size recommended by RFC 2104: the hash's block size
    private const int KeySizeBytes = 64;

    private readonly SemaphoreSlim _lock = new(1, 1);
    private byte[]? _key;

    public async Task<byte[]> GetKeyAsync(CancellationToken ct = default)
    {
        if (_key is not null)
            return _key;

        await _lock.WaitAsync(ct);
        try
        {
            return _key ??= await LoadOrCreateAsync(ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<byte[]> LoadOrCreateAsync(CancellationToken ct)
    {
        var container = blobs.GetBlobContainerClient(ContainerName);
        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        var blob = container.GetBlobClient(BlobName);

        try
        {
            // Only the first instance wins; the others get 409 and read the winner's key
            var conditions = new BlobRequestConditions { IfNoneMatch = ETag.All };
            await blob.UploadAsync(BinaryData.FromBytes(RandomNumberGenerator.GetBytes(KeySizeBytes)),
                new BlobUploadOptions { Conditions = conditions }, ct);
        }
        catch (RequestFailedException ex) when (ex.Status is 409 or 412)
        {
            // Already there
        }

        var content = await blob.DownloadContentAsync(ct);
        return content.Value.Content.ToArray();
    }
}
