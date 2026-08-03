using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PosterFly.Models;

public class Collection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public List<ApiRequest> Requests { get; set; } = new();
    
    public List<CollectionFolder> Folders { get; set; } = new();
    
    public Dictionary<string, string> Variables { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModified { get; set; }
    
    public string? AuthType { get; set; } // Bearer, Basic, API Key, OAuth2, etc.
    public Dictionary<string, string> AuthSettings { get; set; } = new();
}

public class CollectionFolder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public List<ApiRequest> Requests { get; set; } = new();
    
    public List<CollectionFolder> SubFolders { get; set; } = new();
    
    public Guid CollectionId { get; set; }
    
    public Guid? ParentFolderId { get; set; }
}