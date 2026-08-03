using System.Text.Json.Serialization;

namespace PosterFly.Models;

public class ApiResponse
{
    public Guid RequestId { get; set; }
    
    public int StatusCode { get; set; }
    
    public string StatusText { get; set; } = string.Empty;
    
    public Dictionary<string, string> Headers { get; set; } = new();
    
    public string Body { get; set; } = string.Empty;
    
    public long ResponseTimeMs { get; set; }
    
    public long ContentLength { get; set; }
    
    public string ContentType { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    
    public string? ErrorMessage { get; set; }
    
    public Exception? Exception { get; set; }
    
    // Additional response metadata
    public string Protocol { get; set; } = "HTTP/1.1";
    public string? ServerInfo { get; set; }
    public bool FromCache { get; set; } = false;
}