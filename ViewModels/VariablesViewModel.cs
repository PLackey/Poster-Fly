using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using PosterFly.Models;
using PosterFly.Services;
using Environment = PosterFly.Models.Environment;

namespace PosterFly.ViewModels;

public class VariablesViewModel : INotifyPropertyChanged
{
    private readonly IVariableService _variableService;
    private readonly ILogger<VariablesViewModel> _logger;
    
    private ObservableCollection<Variable> _allVariables = new();
    private ObservableCollection<Variable> _filteredVariables = new();
    private ObservableCollection<Environment> _environments = new();
    private Environment? _activeEnvironment;
    private string _searchText = string.Empty;
    private bool _isLoading;
    
    // New variable form
    private string _newVariableName = string.Empty;
    private string _newVariableValue = string.Empty;
    private string _newVariableDescription = string.Empty;
    private string _newVariableScope = "Global";
    private bool _newVariableIsSecret;
    
    // View state
    private bool _showingGlobal = true;
    private bool _showingEnvironment;
    private bool _showingMostUsed;

    public VariablesViewModel(IVariableService variableService, ILogger<VariablesViewModel> logger)
    {
        _variableService = variableService;
        _logger = logger;
        
        // Commands
        AddVariableCommand = new Command(async () => await AddVariableAsync(), () => CanAddVariable());
        EditVariableCommand = new Command<Variable>(async (variable) => await EditVariableAsync(variable));
        DeleteVariableCommand = new Command<Variable>(async (variable) => await DeleteVariableAsync(variable));
        CopyVariableCommand = new Command<Variable>(async (variable) => await CopyVariableAsync(variable));
        SearchCommand = new Command(async () => await SearchVariablesAsync());
        
        ShowGlobalVariablesCommand = new Command(async () => await ShowGlobalVariablesAsync());
        ShowEnvironmentVariablesCommand = new Command(async () => await ShowEnvironmentVariablesAsync());
        ShowMostUsedCommand = new Command(async () => await ShowMostUsedAsync());
        
        NewEnvironmentCommand = new Command(async () => await CreateEnvironmentAsync());
        ManageEnvironmentsCommand = new Command(async () => await ManageEnvironmentsAsync());
        
        // Fire and forget - we want this to run in the background
        _ = Task.Run(async () => await LoadDataAsync());
    }

    // Properties
    public ObservableCollection<Variable> FilteredVariables
    {
        get => _filteredVariables;
        set { _filteredVariables = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoVariables)); }
    }

    public ObservableCollection<Environment> Environments
    {
        get => _environments;
        set { _environments = value; OnPropertyChanged(); }
    }

    public Environment? ActiveEnvironment
    {
        get => _activeEnvironment;
        set { _activeEnvironment = value; OnPropertyChanged(); OnActiveEnvironmentChanged(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool HasNoVariables => !FilteredVariables.Any();

    // New variable form properties
    public string NewVariableName
    {
        get => _newVariableName;
        set { _newVariableName = value; OnPropertyChanged(); ((Command)AddVariableCommand).ChangeCanExecute(); }
    }

    public string NewVariableValue
    {
        get => _newVariableValue;
        set { _newVariableValue = value; OnPropertyChanged(); ((Command)AddVariableCommand).ChangeCanExecute(); }
    }

    public string NewVariableDescription
    {
        get => _newVariableDescription;
        set { _newVariableDescription = value; OnPropertyChanged(); }
    }

    public string NewVariableScope
    {
        get => _newVariableScope;
        set { _newVariableScope = value; OnPropertyChanged(); }
    }

    public bool NewVariableIsSecret
    {
        get => _newVariableIsSecret;
        set { _newVariableIsSecret = value; OnPropertyChanged(); }
    }

    // View state properties
    public bool ShowingGlobal
    {
        get => _showingGlobal;
        set { _showingGlobal = value; OnPropertyChanged(); }
    }

    public bool ShowingEnvironment
    {
        get => _showingEnvironment;
        set { _showingEnvironment = value; OnPropertyChanged(); }
    }

    public bool ShowingMostUsed
    {
        get => _showingMostUsed;
        set { _showingMostUsed = value; OnPropertyChanged(); }
    }

    // Collections for UI
    public List<string> VariableScopes => Enum.GetNames(typeof(VariableScope)).ToList();

    // Commands
    public ICommand AddVariableCommand { get; }
    public ICommand EditVariableCommand { get; }
    public ICommand DeleteVariableCommand { get; }
    public ICommand CopyVariableCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ShowGlobalVariablesCommand { get; }
    public ICommand ShowEnvironmentVariablesCommand { get; }
    public ICommand ShowMostUsedCommand { get; }
    public ICommand NewEnvironmentCommand { get; }
    public ICommand ManageEnvironmentsCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Load variables and environments
            var variables = await _variableService.GetAllVariablesAsync();
            var environments = await _variableService.GetEnvironmentsAsync();
            var activeEnvironment = await _variableService.GetActiveEnvironmentAsync();
            
            _allVariables.Clear();
            foreach (var variable in variables)
            {
                _allVariables.Add(variable);
            }
            
            Environments.Clear();
            foreach (var env in environments)
            {
                Environments.Add(env);
            }
            
            ActiveEnvironment = activeEnvironment;
            
            // Show global variables by default
            await ShowGlobalVariablesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading variables data");
            await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to load variables", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddVariable()
    {
        return !string.IsNullOrWhiteSpace(NewVariableName) && !string.IsNullOrWhiteSpace(NewVariableValue);
    }

    private async Task AddVariableAsync()
    {
        if (!CanAddVariable()) return;

        try
        {
            var variable = new Variable
            {
                Name = NewVariableName.Trim(),
                Value = NewVariableValue.Trim(),
                Description = string.IsNullOrWhiteSpace(NewVariableDescription) ? null : NewVariableDescription.Trim(),
                Scope = Enum.Parse<VariableScope>(NewVariableScope),
                IsSecret = NewVariableIsSecret,
                CollectionId = NewVariableScope == "Collection" ? "default" : null,
                EnvironmentId = NewVariableScope == "Environment" ? ActiveEnvironment?.Id.ToString() : null
            };

            await _variableService.SaveVariableAsync(variable);
            _allVariables.Add(variable);

            // Clear form
            NewVariableName = string.Empty;
            NewVariableValue = string.Empty;
            NewVariableDescription = string.Empty;
            NewVariableIsSecret = false;

            await RefreshCurrentView();
            await Application.Current?.MainPage?.DisplayAlert("Success", "Variable added successfully!", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding variable");
            await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to add variable", "OK");
        }
    }

    private async Task EditVariableAsync(Variable variable)
    {
        if (variable == null) return;

        var name = await Application.Current?.MainPage?.DisplayPromptAsync(
            "Edit Variable",
            "Enter new name:",
            "Save",
            "Cancel",
            variable.Name);

        if (!string.IsNullOrWhiteSpace(name) && name != variable.Name)
        {
            var value = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Edit Variable",
                "Enter new value:",
                "Save",
                "Cancel",
                variable.Value);

            if (!string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    variable.Name = name.Trim();
                    variable.Value = value.Trim();
                    await _variableService.SaveVariableAsync(variable);
                    
                    await RefreshCurrentView();
                    await Application.Current?.MainPage?.DisplayAlert("Success", "Variable updated successfully!", "OK");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating variable");
                    await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to update variable", "OK");
                }
            }
        }
    }

    private async Task DeleteVariableAsync(Variable variable)
    {
        if (variable == null) return;
        if (Application.Current?.MainPage == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Delete Variable",
            $"Are you sure you want to delete '{variable.Name}'?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            try
            {
                await _variableService.DeleteVariableAsync(variable.Id);
                _allVariables.Remove(variable);
                FilteredVariables.Remove(variable);
                
                await Application.Current?.MainPage?.DisplayAlert("Success", "Variable deleted successfully!", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting variable");
                await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to delete variable", "OK");
            }
        }
    }

    private async Task CopyVariableAsync(Variable variable)
    {
        if (variable == null) return;

        try
        {
            var variableReference = $"{{{{{variable.Name}}}}}";
            await Clipboard.SetTextAsync(variableReference);
            await Application.Current?.MainPage?.DisplayAlert("Copied", $"Variable reference copied: {variableReference}", "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying variable");
            await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to copy variable", "OK");
        }
    }

    private async Task SearchVariablesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await RefreshCurrentView();
            return;
        }

        try
        {
            var searchResults = await _variableService.SearchVariablesAsync(SearchText);
            FilteredVariables.Clear();
            foreach (var variable in searchResults)
            {
                FilteredVariables.Add(variable);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching variables");
        }
    }

    private Task ShowGlobalVariablesAsync()
    {
        ShowingGlobal = true;
        ShowingEnvironment = false;
        ShowingMostUsed = false;
        
        FilteredVariables.Clear();
        foreach (var variable in _allVariables.Where(v => v.Scope == VariableScope.Global))
        {
            FilteredVariables.Add(variable);
        }
        
        return Task.CompletedTask;
    }

    private Task ShowEnvironmentVariablesAsync()
    {
        ShowingGlobal = false;
        ShowingEnvironment = true;
        ShowingMostUsed = false;
        
        FilteredVariables.Clear();
        if (ActiveEnvironment != null)
        {
            foreach (var variable in ActiveEnvironment.Variables)
            {
                FilteredVariables.Add(variable);
            }
        }
        
        return Task.CompletedTask;
    }

    private async Task ShowMostUsedAsync()
    {
        ShowingGlobal = false;
        ShowingEnvironment = false;
        ShowingMostUsed = true;
        
        try
        {
            var mostUsed = await _variableService.GetVariablesByUsageAsync(20);
            FilteredVariables.Clear();
            foreach (var variable in mostUsed)
            {
                FilteredVariables.Add(variable);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading most used variables");
        }
    }

    private async Task RefreshCurrentView()
    {
        if (ShowingGlobal)
            await ShowGlobalVariablesAsync();
        else if (ShowingEnvironment)
            await ShowEnvironmentVariablesAsync();
        else if (ShowingMostUsed)
            await ShowMostUsedAsync();
    }

    private async Task CreateEnvironmentAsync()
    {
        var name = await Application.Current?.MainPage?.DisplayPromptAsync(
            "New Environment",
            "Enter environment name:",
            "Create",
            "Cancel");

        if (!string.IsNullOrWhiteSpace(name))
        {
            try
            {
                var environment = new Environment
                {
                    Name = name.Trim(),
                    Description = $"Environment created on {DateTime.Now:MMM dd, yyyy}"
                };

                await _variableService.SaveEnvironmentAsync(environment);
                Environments.Add(environment);
                
                await Application.Current?.MainPage?.DisplayAlert("Success", "Environment created successfully!", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating environment");
                await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to create environment", "OK");
            }
        }
    }

    private async Task ManageEnvironmentsAsync()
    {
        await Application.Current?.MainPage?.DisplayAlert(
            "Manage Environments", 
            "Environment management UI would be implemented here.\n\nFeatures:\n- Rename environments\n- Delete environments\n- Duplicate environments\n- Import/Export environments", 
            "OK");
    }

    private async void OnActiveEnvironmentChanged()
    {
        if (ActiveEnvironment != null)
        {
            try
            {
                await _variableService.SetActiveEnvironmentAsync(ActiveEnvironment.Id);
                if (ShowingEnvironment)
                {
                    await ShowEnvironmentVariablesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting active environment");
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}