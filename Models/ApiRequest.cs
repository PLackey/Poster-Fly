using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PosterFly.Models;

public enum RequestType
{
    HTTP,
    GRPC,
    GraphQL
}

public enum HttpMethod
{
    GET,
    POST,
    PUT,
    DELETE,
    PATCH,
    HEAD,
    OPTIONS
}

public class ApiRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Url { get; set; } = string.Empty;
    
    public RequestType Type { get; set; } = RequestType.HTTP;
    
    public HttpMethod Method { get; set; } = HttpMethod.GET;
    
    public Dictionary<string, string> Headers { get; set; } = new();
    
    public string Body { get; set; } = string.Empty;
    
    public string? BodyType { get; set; } = "JSON"; // JSON, XML, Text, Form, Binary
    
    public bool UseSSL { get; set; } = true;
    
    public bool UseHttp2 { get; set; } = false;
    
    public int TimeoutSeconds { get; set; } = 30;
    
    // GraphQL specific
    public string? GraphQLQuery { get; set; }
    public string? GraphQLVariables { get; set; }
    
    // gRPC specific
    public string? GrpcService { get; set; }
    public string? GrpcMethod { get; set; }
    public string? ProtoFile { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    
    // Collection/Folder organization
    public string? CollectionId { get; set; }
    public string? FolderPath { get; set; }
}