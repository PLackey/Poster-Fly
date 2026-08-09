using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using PosterFly.Models;
using HttpMethod = PosterFly.Models.HttpMethod;

namespace PosterFly.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse> SendHttpRequestAsync(ApiRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new ApiResponse { RequestId = request.Id };

        try
        {
            using var httpRequest = CreateHttpRequestMessage(request);
            
            // Configure HTTP version
            if (request.UseHttp2)
            {
                httpRequest.Version = HttpVersion.Version20;
                httpRequest.VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            }

            using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            
            stopwatch.Stop();
            
            response.StatusCode = (int)httpResponse.StatusCode;
            response.StatusText = httpResponse.ReasonPhrase ?? string.Empty;
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.Protocol = $"HTTP/{httpResponse.Version}";

            // Read headers
            foreach (var header in httpResponse.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }
            
            foreach (var header in httpResponse.Content.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }

            // Read content
            response.Body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            response.ContentLength = response.Body.Length;
            response.ContentType = httpResponse.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (response.Headers.ContainsKey("Server"))
                response.ServerInfo = response.Headers["Server"];

        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.StatusCode = 0;
            response.ErrorMessage = ex.Message;
            response.Exception = ex;
            _logger.LogError(ex, "Error sending HTTP request to {Url}", request.Url);
        }

        return response;
    }

    public async Task<ApiResponse> SendGraphQLRequestAsync(ApiRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new ApiResponse { RequestId = request.Id };

        try
        {
            var graphQLClient = new GraphQLHttpClient(request.Url, new NewtonsoftJsonSerializer());
            
            // Add headers
            foreach (var header in request.Headers)
            {
                graphQLClient.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            var graphQLRequest = new GraphQLRequest
            {
                Query = request.GraphQLQuery ?? request.Body,
                Variables = string.IsNullOrEmpty(request.GraphQLVariables) ? null : 
                           JsonSerializer.Deserialize<object>(request.GraphQLVariables)
            };

            var graphQLResponse = await graphQLClient.SendQueryAsync<object>(graphQLRequest, cancellationToken);
            
            stopwatch.Stop();
            
            response.StatusCode = 200; // GraphQL typically returns 200 even for errors
            response.StatusText = "OK";
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.Protocol = "HTTP/1.1";
            
            if (graphQLResponse.Errors != null && graphQLResponse.Errors.Any())
            {
                response.ErrorMessage = string.Join("; ", graphQLResponse.Errors.Select(e => e.Message));
            }

            response.Body = JsonSerializer.Serialize(new 
            { 
                data = graphQLResponse.Data, 
                errors = graphQLResponse.Errors 
            }, new JsonSerializerOptions { WriteIndented = true });
            
            response.ContentLength = response.Body.Length;
            response.ContentType = "application/json";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.StatusCode = 0;
            response.ErrorMessage = ex.Message;
            response.Exception = ex;
            _logger.LogError(ex, "Error sending GraphQL request to {Url}", request.Url);
        }

        return response;
    }

    public Task<ApiResponse> SendGrpcRequestAsync(ApiRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new ApiResponse { RequestId = request.Id };

        try
        {
            // This is a simplified gRPC implementation
            // In a real scenario, you'd need to load proto files and generate clients
            var channel = GrpcChannel.ForAddress(request.Url);
            
            stopwatch.Stop();
            
            response.StatusCode = 200;
            response.StatusText = "OK";
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.Protocol = "HTTP/2";
            response.Body = "gRPC request would be sent here. Proto file compilation and dynamic invocation required.";
            response.ContentType = "application/grpc";
            response.ContentLength = response.Body.Length;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            response.StatusCode = 0;
            response.ErrorMessage = ex.Message;
            response.Exception = ex;
            _logger.LogError(ex, "Error sending gRPC request to {Url}", request.Url);
        }

        return Task.FromResult(response);
    }

    public async Task<bool> TestConnectionAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Head, url);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private HttpRequestMessage CreateHttpRequestMessage(ApiRequest request)
    {
        var httpMethod = request.Method switch
        {
            HttpMethod.GET => System.Net.Http.HttpMethod.Get,
            HttpMethod.POST => System.Net.Http.HttpMethod.Post,
            HttpMethod.PUT => System.Net.Http.HttpMethod.Put,
            HttpMethod.DELETE => System.Net.Http.HttpMethod.Delete,
            HttpMethod.PATCH => System.Net.Http.HttpMethod.Patch,
            HttpMethod.HEAD => System.Net.Http.HttpMethod.Head,
            HttpMethod.OPTIONS => System.Net.Http.HttpMethod.Options,
            _ => System.Net.Http.HttpMethod.Get
        };

        var httpRequest = new HttpRequestMessage(httpMethod, request.Url);

        // Add headers
        foreach (var header in request.Headers)
        {
            if (!httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                // Some headers need to be added to content headers
                if (httpRequest.Content == null)
                    httpRequest.Content = new StringContent("", Encoding.UTF8);
                
                httpRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Add body for methods that support it
        if (request.Method is HttpMethod.POST or HttpMethod.PUT or HttpMethod.PATCH && !string.IsNullOrEmpty(request.Body))
        {
            var contentType = request.BodyType?.ToLower() switch
            {
                "json" => "application/json",
                "xml" => "application/xml",
                "form" => "application/x-www-form-urlencoded",
                "text" => "text/plain",
                _ => "application/json"
            };

            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, contentType);
        }

        return httpRequest;
    }
}