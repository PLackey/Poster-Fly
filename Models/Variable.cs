using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PosterFly.Models;

public class Variable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public VariableScope Scope { get; set; } = VariableScope.Global;
    
    public string? CollectionId { get; set; }
    
    public string? EnvironmentId { get; set; }
    
    public int UsageCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastUsed { get; set; } = DateTime.UtcNow;
    
    public bool IsSecret { get; set; } = false;
    
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum VariableScope
{
    Global,
    Collection,
    Environment,
    Request
}

public class Environment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public List<Variable> Variables { get; set; } = new();
    
    public bool IsActive { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastModified { get; set; }
}