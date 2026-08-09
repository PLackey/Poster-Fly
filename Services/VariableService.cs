using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PosterFly.Models;
using Environment = PosterFly.Models.Environment;

namespace PosterFly.Services;

public class VariableService : IVariableService
{
    private readonly string _variablesPath;
    private readonly string _environmentsPath;
    private readonly ILogger<VariableService> _logger;
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly Regex _variablePattern = new(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);

    public VariableService(ILogger<VariableService> logger)
    {
        _logger = logger;
        var appDataPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "PosterFly");
        Directory.CreateDirectory(appDataPath);
        
        _variablesPath = Path.Combine(appDataPath, "variables.json");
        _environmentsPath = Path.Combine(appDataPath, "environments.json");
    }

    public async Task<List<Variable>> GetAllVariablesAsync()
    {
        try
        {
            if (!File.Exists(_variablesPath))
                return new List<Variable>();

            var json = await File.ReadAllTextAsync(_variablesPath);
            return JsonSerializer.Deserialize<List<Variable>>(json, _jsonOptions) ?? new List<Variable>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading variables");
            return new List<Variable>();
        }
    }

    public async Task<List<Variable>> GetVariablesByUsageAsync(int count = 20)
    {
        var variables = await GetAllVariablesAsync();
        return variables
            .OrderByDescending(v => v.UsageCount)
            .ThenByDescending(v => v.LastUsed)
            .Take(count)
            .ToList();
    }

    public async Task<List<Variable>> SearchVariablesAsync(string searchTerm)
    {
        var variables = await GetAllVariablesAsync();
        return variables
            .Where(v => v.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       (v.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(v => v.Name)
            .ToList();
    }

    public async Task<Variable> SaveVariableAsync(Variable variable)
    {
        try
        {
            var variables = await GetAllVariablesAsync();
            
            var existing = variables.FirstOrDefault(v => v.Id == variable.Id);
            if (existing != null)
            {
                variables.Remove(existing);
            }
            
            variables.Add(variable);
            
            var json = JsonSerializer.Serialize(variables, _jsonOptions);
            await File.WriteAllTextAsync(_variablesPath, json);
            
            return variable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving variable");
            throw;
        }
    }

    public async Task DeleteVariableAsync(Guid variableId)
    {
        try
        {
            var variables = await GetAllVariablesAsync();
            variables.RemoveAll(v => v.Id == variableId);
            
            var json = JsonSerializer.Serialize(variables, _jsonOptions);
            await File.WriteAllTextAsync(_variablesPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting variable");
            throw;
        }
    }

    public async Task IncrementUsageAsync(string variableName)
    {
        try
        {
            var variables = await GetAllVariablesAsync();
            var variable = variables.FirstOrDefault(v => v.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase));
            
            if (variable != null)
            {
                variable.UsageCount++;
                variable.LastUsed = DateTime.UtcNow;
                await SaveVariableAsync(variable);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing variable usage");
        }
    }

    public async Task<List<Environment>> GetEnvironmentsAsync()
    {
        try
        {
            if (!File.Exists(_environmentsPath))
                return new List<Environment>();

            var json = await File.ReadAllTextAsync(_environmentsPath);
            return JsonSerializer.Deserialize<List<Environment>>(json, _jsonOptions) ?? new List<Environment>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading environments");
            return new List<Environment>();
        }
    }

    public async Task<Environment> SaveEnvironmentAsync(Environment environment)
    {
        try
        {
            var environments = await GetEnvironmentsAsync();
            
            var existing = environments.FirstOrDefault(e => e.Id == environment.Id);
            if (existing != null)
            {
                environments.Remove(existing);
            }
            
            environment.LastModified = DateTime.UtcNow;
            environments.Add(environment);
            
            var json = JsonSerializer.Serialize(environments, _jsonOptions);
            await File.WriteAllTextAsync(_environmentsPath, json);
            
            return environment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving environment");
            throw;
        }
    }

    public async Task DeleteEnvironmentAsync(Guid environmentId)
    {
        try
        {
            var environments = await GetEnvironmentsAsync();
            environments.RemoveAll(e => e.Id == environmentId);
            
            var json = JsonSerializer.Serialize(environments, _jsonOptions);
            await File.WriteAllTextAsync(_environmentsPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting environment");
            throw;
        }
    }

    public async Task<Environment?> GetActiveEnvironmentAsync()
    {
        var environments = await GetEnvironmentsAsync();
        return environments.FirstOrDefault(e => e.IsActive);
    }

    public async Task SetActiveEnvironmentAsync(Guid environmentId)
    {
        try
        {
            var environments = await GetEnvironmentsAsync();
            
            // Deactivate all environments
            foreach (var env in environments)
            {
                env.IsActive = false;
            }
            
            // Activate the selected environment
            var targetEnv = environments.FirstOrDefault(e => e.Id == environmentId);
            if (targetEnv != null)
            {
                targetEnv.IsActive = true;
            }
            
            var json = JsonSerializer.Serialize(environments, _jsonOptions);
            await File.WriteAllTextAsync(_environmentsPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting active environment");
            throw;
        }
    }

    public string ResolveVariables(string input, Environment? environment = null, string? collectionId = null)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var variables = GetAllVariablesAsync().Result;
        
        // Merge variables based on scope priority: Global < Collection < Environment
        var resolvedVars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        // Add global variables
        foreach (var var in variables.Where(v => v.Scope == VariableScope.Global))
        {
            resolvedVars[var.Name] = var.Value;
        }
        
        // Add collection variables
        if (!string.IsNullOrEmpty(collectionId))
        {
            foreach (var var in variables.Where(v => v.Scope == VariableScope.Collection && v.CollectionId == collectionId))
            {
                resolvedVars[var.Name] = var.Value;
            }
        }
        
        // Add environment variables (highest priority)
        if (environment != null)
        {
            foreach (var var in environment.Variables)
            {
                resolvedVars[var.Name] = var.Value;
            }
        }

        // Replace variables in the input
        return _variablePattern.Replace(input, match =>
        {
            var variableName = match.Groups[1].Value.Trim();
            
            // Increment usage counter for resolved variables
            _ = Task.Run(() => IncrementUsageAsync(variableName));
            
            return resolvedVars.TryGetValue(variableName, out var value) ? value : match.Value;
        });
    }

    public List<string> ExtractVariableNames(string input)
    {
        if (string.IsNullOrEmpty(input))
            return new List<string>();

        var matches = _variablePattern.Matches(input);
        return matches.Cast<Match>()
            .Select(m => m.Groups[1].Value.Trim())
            .Distinct()
            .ToList();
    }

    public async Task<List<string>> GetAutoCompleteVariablesAsync(string prefix)
    {
        var variables = await GetVariablesByUsageAsync(100);
        
        return variables
            .Where(v => v.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(v => v.UsageCount)
            .ThenBy(v => v.Name)
            .Select(v => v.Name)
            .Take(10)
            .ToList();
    }
}