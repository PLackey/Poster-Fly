using System.Text.Json;
using Microsoft.Extensions.Logging;
using PosterFly.Models;

namespace PosterFly.Services;

public class StorageService : IStorageService
{
    private readonly string _collectionsPath;
    private readonly string _historyPath;
    private readonly ILogger<StorageService> _logger;
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public StorageService(ILogger<StorageService> logger)
    {
        _logger = logger;
        var appDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "PosterFly");
        Directory.CreateDirectory(appDataPath);
        
        _collectionsPath = Path.Combine(appDataPath, "collections");
        _historyPath = Path.Combine(appDataPath, "history");
        
        Directory.CreateDirectory(_collectionsPath);
        Directory.CreateDirectory(_historyPath);
    }

    public async Task<List<Collection>> LoadCollectionsAsync()
    {
        try
        {
            var collections = new List<Collection>();
            var files = Directory.GetFiles(_collectionsPath, "*.json");

            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var collection = JsonSerializer.Deserialize<Collection>(json, _jsonOptions);
                if (collection != null)
                    collections.Add(collection);
            }

            return collections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading collections");
            return new List<Collection>();
        }
    }

    public async Task SaveCollectionAsync(Collection collection)
    {
        try
        {
            collection.LastModified = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(collection, _jsonOptions);
            var fileName = $"{collection.Id}.json";
            var filePath = Path.Combine(_collectionsPath, fileName);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving collection {CollectionId}", collection.Id);
            throw;
        }
    }

    public Task DeleteCollectionAsync(Guid collectionId)
    {
        try
        {
            var fileName = $"{collectionId}.json";
            var filePath = Path.Combine(_collectionsPath, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting collection {CollectionId}", collectionId);
            throw;
        }
        
        return Task.CompletedTask;
    }

    public async Task<string> ExportCollectionAsync(Collection collection, string format = "json")
    {
        try
        {
            return format.ToLower() switch
            {
                "json" => JsonSerializer.Serialize(collection, _jsonOptions),
                "postman" => await ConvertToPostmanAsync(collection),
                _ => throw new NotSupportedException($"Export format '{format}' is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting collection {CollectionId}", collection.Id);
            throw;
        }
    }

    public async Task<Collection?> ImportCollectionAsync(string content, string format = "json")
    {
        try
        {
            return format.ToLower() switch
            {
                "json" => JsonSerializer.Deserialize<Collection>(content, _jsonOptions),
                "postman" => await ConvertFromPostmanAsync(content),
                _ => throw new NotSupportedException($"Import format '{format}' is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing collection");
            throw;
        }
    }

    public async Task<List<ApiRequest>> LoadRecentRequestsAsync(int count = 10)
    {
        try
        {
            var requests = new List<ApiRequest>();
            var files = Directory.GetFiles(_historyPath, "*.json")
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .Take(count);

            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var request = JsonSerializer.Deserialize<ApiRequest>(json, _jsonOptions);
                if (request != null)
                    requests.Add(request);
            }

            return requests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recent requests");
            return new List<ApiRequest>();
        }
    }

    public async Task SaveRequestHistoryAsync(ApiRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var fileName = $"{request.Id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_historyPath, fileName);
            await File.WriteAllTextAsync(filePath, json);

            // Clean up old history files (keep last 100)
            var files = Directory.GetFiles(_historyPath, "*.json")
                .OrderByDescending(f => new FileInfo(f).CreationTime)
                .Skip(100);

            foreach (var oldFile in files)
            {
                File.Delete(oldFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving request history");
        }
    }

    private async Task<string> ConvertToPostmanAsync(Collection collection)
    {
        // Full Postman export format compatible with Postman v2.1
        var postmanCollection = new
        {
            info = new
            {
                _postman_id = collection.Id.ToString(),
                name = collection.Name,
                description = collection.Description ?? "",
                schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json",
                _exporter_id = "PosterFly"
            },
            item = await ConvertRequestsToPostmanItems(collection.Requests, collection.Folders),
            auth = ConvertAuthToPostman(collection),
            @event = new[]
            {
                new
                {
                    listen = "prerequest",
                    script = new
                    {
                        type = "text/javascript",
                        exec = new[] { "// Pre-request script" }
                    }
                },
                new
                {
                    listen = "test",
                    script = new
                    {
                        type = "text/javascript",
                        exec = new[] { "// Test script" }
                    }
                }
            },
            variable = collection.Variables.Select(v => new
            {
                key = v.Key,
                value = v.Value,
                type = "string"
            })
        };

        return JsonSerializer.Serialize(postmanCollection, _jsonOptions);
    }

    private async Task<Collection> ConvertFromPostmanAsync(string content)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        var collection = new Collection();

        // Parse collection info
        if (root.TryGetProperty("info", out var info))
        {
            collection.Name = info.TryGetProperty("name", out var name) ? name.GetString() ?? "Imported Collection" : "Imported Collection";
            collection.Description = info.TryGetProperty("description", out var desc) ? desc.GetString() : null;
        }

        // Parse collection variables
        if (root.TryGetProperty("variable", out var variables))
        {
            foreach (var variable in variables.EnumerateArray())
            {
                var key = variable.TryGetProperty("key", out var keyProp) ? keyProp.GetString() : null;
                var value = variable.TryGetProperty("value", out var valueProp) ? valueProp.GetString() : null;
                
                if (key != null && value != null)
                {
                    collection.Variables[key] = value;
                }
            }
        }

        // Parse collection auth
        if (root.TryGetProperty("auth", out var auth))
        {
            ParsePostmanAuth(auth, collection);
        }

        // Parse items (requests and folders)
        if (root.TryGetProperty("item", out var items))
        {
            await ParsePostmanItems(items, collection, null);
        }

        return collection;
    }

    private async Task<object[]> ConvertRequestsToPostmanItems(List<ApiRequest> requests, List<CollectionFolder> folders)
    {
        var items = new List<object>();

        // Add requests
        foreach (var request in requests)
        {
            var postmanRequest = new
            {
                name = request.Name,
                request = new
                {
                    method = request.Method.ToString().ToUpper(),
                    header = request.Headers.Select(h => new 
                    { 
                        key = h.Key, 
                        value = h.Value, 
                        type = "text" 
                    }).ToArray(),
                    url = ParsePostmanUrl(request.Url),
                    body = CreatePostmanBody(request),
                    description = new { content = "", type = "text/plain" }
                },
                response = new object[0] // Empty responses array
            };
            items.Add(postmanRequest);
        }

        // Add folders
        foreach (var folder in folders)
        {
            var postmanFolder = new
            {
                name = folder.Name,
                description = folder.Description ?? "",
                item = await ConvertRequestsToPostmanItems(folder.Requests, folder.SubFolders)
            };
            items.Add(postmanFolder);
        }

        return items.ToArray();
    }

    private object ParsePostmanUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var queryParams = new List<object>();
            
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var query = uri.Query.TrimStart('?');
                var pairs = query.Split('&');
                
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        queryParams.Add(new
                        {
                            key = Uri.UnescapeDataString(keyValue[0]),
                            value = Uri.UnescapeDataString(keyValue[1])
                        });
                    }
                }
            }

            return new
            {
                raw = url,
                protocol = uri.Scheme,
                host = uri.Host.Split('.'),
                port = uri.Port != -1 ? uri.Port.ToString() : null,
                path = uri.AbsolutePath.Split('/').Where(s => !string.IsNullOrEmpty(s)).ToArray(),
                query = queryParams.ToArray()
            };
        }
        catch
        {
            return new { raw = url };
        }
    }

    private object CreatePostmanBody(ApiRequest request)
    {
        if (string.IsNullOrEmpty(request.Body))
        {
            return new { mode = "raw", raw = "" };
        }

        var bodyType = request.BodyType?.ToLower();
        return bodyType switch
        {
            "json" => new 
            { 
                mode = "raw", 
                raw = request.Body,
                options = new { raw = new { language = "json" } }
            },
            "xml" => new 
            { 
                mode = "raw", 
                raw = request.Body,
                options = new { raw = new { language = "xml" } }
            },
            "form" => new 
            { 
                mode = "urlencoded",
                urlencoded = ParseFormData(request.Body)
            },
            "graphql" => new
            {
                mode = "graphql",
                graphql = new
                {
                    query = request.GraphQLQuery ?? request.Body,
                    variables = request.GraphQLVariables ?? "{}"
                }
            },
            _ => new { mode = "raw", raw = request.Body }
        };
    }

    private object[] ParseFormData(string formData)
    {
        var pairs = formData.Split('&');
        return pairs.Select(pair =>
        {
            var keyValue = pair.Split('=');
            return new
            {
                key = keyValue.Length > 0 ? Uri.UnescapeDataString(keyValue[0]) : "",
                value = keyValue.Length > 1 ? Uri.UnescapeDataString(keyValue[1]) : "",
                type = "text"
            };
        }).ToArray();
    }

    private void ParsePostmanAuth(JsonElement auth, Collection collection)
    {
        if (auth.TryGetProperty("type", out var authType))
        {
            collection.AuthType = authType.GetString();
            
            // Parse auth details based on type
            var authSettings = new Dictionary<string, string>();
            
            switch (authType.GetString()?.ToLower())
            {
                case "bearer":
                    if (auth.TryGetProperty("bearer", out var bearer))
                    {
                        foreach (var prop in bearer.EnumerateArray())
                        {
                            if (prop.TryGetProperty("key", out var key) && prop.TryGetProperty("value", out var value))
                            {
                                authSettings[key.GetString() ?? ""] = value.GetString() ?? "";
                            }
                        }
                    }
                    break;
                
                case "basic":
                    if (auth.TryGetProperty("basic", out var basic))
                    {
                        foreach (var prop in basic.EnumerateArray())
                        {
                            if (prop.TryGetProperty("key", out var key) && prop.TryGetProperty("value", out var value))
                            {
                                authSettings[key.GetString() ?? ""] = value.GetString() ?? "";
                            }
                        }
                    }
                    break;
                
                case "apikey":
                    if (auth.TryGetProperty("apikey", out var apikey))
                    {
                        foreach (var prop in apikey.EnumerateArray())
                        {
                            if (prop.TryGetProperty("key", out var key) && prop.TryGetProperty("value", out var value))
                            {
                                authSettings[key.GetString() ?? ""] = value.GetString() ?? "";
                            }
                        }
                    }
                    break;
            }
            
            collection.AuthSettings = authSettings;
        }
    }

    private async Task ParsePostmanItems(JsonElement items, Collection collection, CollectionFolder? parentFolder)
    {
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("request", out var requestElement))
            {
                // This is a request
                var request = ParsePostmanRequest(item, collection.Id.ToString());
                
                if (parentFolder != null)
                {
                    parentFolder.Requests.Add(request);
                }
                else
                {
                    collection.Requests.Add(request);
                }
            }
            else if (item.TryGetProperty("item", out var subItems))
            {
                // This is a folder
                var folder = new CollectionFolder
                {
                    Name = item.TryGetProperty("name", out var folderName) ? folderName.GetString() ?? "Unnamed Folder" : "Unnamed Folder",
                    Description = item.TryGetProperty("description", out var folderDesc) ? folderDesc.GetString() : null,
                    CollectionId = collection.Id,
                    ParentFolderId = parentFolder?.Id
                };
                
                await ParsePostmanItems(subItems, collection, folder);
                
                if (parentFolder != null)
                {
                    parentFolder.SubFolders.Add(folder);
                }
                else
                {
                    collection.Folders.Add(folder);
                }
            }
        }
    }

    private ApiRequest ParsePostmanRequest(JsonElement item, string collectionId)
    {
        var request = new ApiRequest
        {
            Name = item.TryGetProperty("name", out var name) ? name.GetString() ?? "Unnamed Request" : "Unnamed Request",
            CollectionId = collectionId
        };

        if (item.TryGetProperty("request", out var req))
        {
            // Parse method
            if (req.TryGetProperty("method", out var method))
            {
                if (Enum.TryParse<PosterFly.Models.HttpMethod>(method.GetString(), true, out var httpMethod))
                    request.Method = httpMethod;
            }

            // Parse URL
            if (req.TryGetProperty("url", out var url))
            {
                if (url.ValueKind == JsonValueKind.String)
                {
                    request.Url = url.GetString() ?? "";
                }
                else if (url.TryGetProperty("raw", out var rawUrl))
                {
                    request.Url = rawUrl.GetString() ?? "";
                }
            }

            // Parse headers
            if (req.TryGetProperty("header", out var headers))
            {
                foreach (var header in headers.EnumerateArray())
                {
                    var key = header.TryGetProperty("key", out var keyProp) ? keyProp.GetString() : null;
                    var value = header.TryGetProperty("value", out var valueProp) ? valueProp.GetString() : null;
                    var disabled = header.TryGetProperty("disabled", out var disabledProp) && disabledProp.GetBoolean();
                    
                    if (key != null && value != null && !disabled)
                    {
                        request.Headers[key] = value;
                    }
                }
            }

            // Parse body
            if (req.TryGetProperty("body", out var body))
            {
                if (body.TryGetProperty("mode", out var mode))
                {
                    var bodyMode = mode.GetString();
                    switch (bodyMode)
                    {
                        case "raw":
                            if (body.TryGetProperty("raw", out var rawBody))
                            {
                                request.Body = rawBody.GetString() ?? "";
                            }
                            if (body.TryGetProperty("options", out var options) && 
                                options.TryGetProperty("raw", out var rawOptions) &&
                                rawOptions.TryGetProperty("language", out var language))
                            {
                                request.BodyType = language.GetString()?.ToUpper() ?? "JSON";
                            }
                            break;
                            
                        case "urlencoded":
                            if (body.TryGetProperty("urlencoded", out var urlencoded))
                            {
                                var formPairs = new List<string>();
                                foreach (var pair in urlencoded.EnumerateArray())
                                {
                                    var key = pair.TryGetProperty("key", out var keyProp) ? keyProp.GetString() : "";
                                    var value = pair.TryGetProperty("value", out var valueProp) ? valueProp.GetString() : "";
                                    var disabled = pair.TryGetProperty("disabled", out var disabledProp) && disabledProp.GetBoolean();
                                    
                                    if (!disabled && !string.IsNullOrEmpty(key))
                                    {
                                        formPairs.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value ?? "")}");
                                    }
                                }
                                request.Body = string.Join("&", formPairs);
                                request.BodyType = "Form";
                            }
                            break;
                            
                        case "graphql":
                            if (body.TryGetProperty("graphql", out var graphql))
                            {
                                if (graphql.TryGetProperty("query", out var query))
                                {
                                    request.GraphQLQuery = query.GetString();
                                }
                                if (graphql.TryGetProperty("variables", out var variables))
                                {
                                    request.GraphQLVariables = variables.GetString();
                                }
                                request.Type = RequestType.GraphQL;
                            }
                            break;
                    }
                }
            }
        }

        // Determine SSL usage from URL
        request.UseSSL = request.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

        return request;
    }

    private object ConvertAuthToPostman(Collection collection)
    {
        if (string.IsNullOrEmpty(collection.AuthType))
        {
            return new { type = "noauth" };
        }

        var authType = collection.AuthType.ToLower();
        var authData = collection.AuthSettings.Select(kvp => new
        {
            key = kvp.Key,
            value = kvp.Value,
            type = "string"
        }).ToArray();

        return authType switch
        {
            "bearer" => new { type = authType, bearer = authData },
            "basic" => new { type = authType, basic = authData },
            "apikey" => new { type = authType, apikey = authData },
            "oauth2" => new { type = authType, oauth2 = authData },
            _ => new { type = authType, custom = authData }
        };
    }
}