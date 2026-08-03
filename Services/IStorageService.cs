using PosterFly.Models;

namespace PosterFly.Services;

public interface IStorageService
{
    Task<List<Collection>> LoadCollectionsAsync();
    Task SaveCollectionAsync(Collection collection);
    Task DeleteCollectionAsync(Guid collectionId);
    Task<string> ExportCollectionAsync(Collection collection, string format = "json");
    Task<Collection?> ImportCollectionAsync(string content, string format = "json");
    Task<List<ApiRequest>> LoadRecentRequestsAsync(int count = 10);
    Task SaveRequestHistoryAsync(ApiRequest request);
}