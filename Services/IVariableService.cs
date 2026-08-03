using PosterFly.Models;

namespace PosterFly.Services;

public interface IVariableService
{
    Task<List<Variable>> GetAllVariablesAsync();
    Task<List<Variable>> GetVariablesByUsageAsync(int count = 20);
    Task<List<Variable>> SearchVariablesAsync(string searchTerm);
    Task<Variable> SaveVariableAsync(Variable variable);
    Task DeleteVariableAsync(Guid variableId);
    Task IncrementUsageAsync(string variableName);
    
    // Environment management
    Task<List<Environment>> GetEnvironmentsAsync();
    Task<Environment> SaveEnvironmentAsync(Environment environment);
    Task DeleteEnvironmentAsync(Guid environmentId);
    Task<Environment?> GetActiveEnvironmentAsync();
    Task SetActiveEnvironmentAsync(Guid environmentId);
    
    // Variable resolution and substitution
    string ResolveVariables(string input, Environment? environment = null, string? collectionId = null);
    List<string> ExtractVariableNames(string input);
    Task<List<string>> GetAutoCompleteVariablesAsync(string prefix);
}