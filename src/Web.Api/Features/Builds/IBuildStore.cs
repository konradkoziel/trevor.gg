namespace Web.Api.Features.Builds;

public interface IBuildStore
{
    Task<StoredBuild?> GetAsync(string id, CancellationToken ct = default);

    // Newest first
    Task<IReadOnlyList<StoredBuild>> ListByOwnerAsync(string ownerId, CancellationToken ct = default);

    // Insert or replace
    Task SaveAsync(StoredBuild build, CancellationToken ct = default);

    Task DeleteAsync(string id, CancellationToken ct = default);
}
