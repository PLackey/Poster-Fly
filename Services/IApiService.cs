using PosterFly.Models;

namespace PosterFly.Services;

public interface IApiService
{
    Task<ApiResponse> SendHttpRequestAsync(ApiRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> SendGraphQLRequestAsync(ApiRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse> SendGrpcRequestAsync(ApiRequest request, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(string url, CancellationToken cancellationToken = default);
}