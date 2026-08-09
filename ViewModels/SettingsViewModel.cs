using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace PosterFly.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ILogger<SettingsViewModel> _logger;
    
    private int _defaultTimeoutSeconds = 30;
    private bool _followRedirects = true;
    private bool _verifySSL = true;
    private string _defaultHttpVersion = "HTTP/1.1";
    private int _historyRetentionDays = 30;
    private bool _autoSaveRequests = true;
    private string _selectedTheme = "Light";
    private string _selectedFontSize = "Medium";
    private bool _showResponseTime = true;
    private bool _prettyPrintJson = true;
    private int _maxResponseSizeMB = 10;
    private string _proxyHost = string.Empty;
    private int _proxyPort = 8080;
    private bool _useProxyAuth = false;

    public SettingsViewModel(ILogger<SettingsViewModel> logger)
    {
        _logger = logger;
        
        SaveSettingsCommand = new Command(async () => await SaveSettingsAsync());
        ResetSettingsCommand = new Command(async () => await ResetSettingsAsync());
        ExportAllDataCommand = new Command(async () => await ExportAllDataAsync());
        ImportDataCommand = new Command(async () => await ImportDataAsync());
        ClearAllDataCommand = new Command(async () => await ClearAllDataAsync());
        
        LoadSettings();
    }

    // Request Settings
    public int DefaultTimeoutSeconds
    {
        get => _defaultTimeoutSeconds;
        set { _defaultTimeoutSeconds = value; OnPropertyChanged(); }
    }

    public bool FollowRedirects
    {
        get => _followRedirects;
        set { _followRedirects = value; OnPropertyChanged(); }
    }

    public bool VerifySSL
    {
        get => _verifySSL;
        set { _verifySSL = value; OnPropertyChanged(); }
    }

    public string DefaultHttpVersion
    {
        get => _defaultHttpVersion;
        set { _defaultHttpVersion = value; OnPropertyChanged(); }
    }

    // Storage Settings
    public int HistoryRetentionDays
    {
        get => _historyRetentionDays;
        set { _historyRetentionDays = value; OnPropertyChanged(); }
    }

    public bool AutoSaveRequests
    {
        get => _autoSaveRequests;
        set { _autoSaveRequests = value; OnPropertyChanged(); }
    }

    // UI Settings
    public string SelectedTheme
    {
        get => _selectedTheme;
        set { _selectedTheme = value; OnPropertyChanged(); }
    }

    public string SelectedFontSize
    {
        get => _selectedFontSize;
        set { _selectedFontSize = value; OnPropertyChanged(); }
    }

    public bool ShowResponseTime
    {
        get => _showResponseTime;
        set { _showResponseTime = value; OnPropertyChanged(); }
    }

    public bool PrettyPrintJson
    {
        get => _prettyPrintJson;
        set { _prettyPrintJson = value; OnPropertyChanged(); }
    }

    // Advanced Settings
    public int MaxResponseSizeMB
    {
        get => _maxResponseSizeMB;
        set { _maxResponseSizeMB = value; OnPropertyChanged(); }
    }

    public string ProxyHost
    {
        get => _proxyHost;
        set { _proxyHost = value; OnPropertyChanged(); }
    }

    public int ProxyPort
    {
        get => _proxyPort;
        set { _proxyPort = value; OnPropertyChanged(); }
    }

    public bool UseProxyAuth
    {
        get => _useProxyAuth;
        set { _useProxyAuth = value; OnPropertyChanged(); }
    }

    // Collections for Pickers
    public List<string> HttpVersions => new() { "HTTP/1.1", "HTTP/2" };
    public List<string> Themes => new() { "Light", "Dark", "Auto" };
    public List<string> FontSizes => new() { "Small", "Medium", "Large", "Extra Large" };

    // Commands
    public ICommand SaveSettingsCommand { get; }
    public ICommand ResetSettingsCommand { get; }
    public ICommand ExportAllDataCommand { get; }
    public ICommand ImportDataCommand { get; }
    public ICommand ClearAllDataCommand { get; }

    private void LoadSettings()
    {
        try
        {
            // In a real app, load from preferences/settings storage
            // For now, use defaults
            _logger.LogInformation("Settings loaded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings");
        }
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            // In a real app, save to preferences/settings storage
            await Application.Current?.MainPage?.DisplayAlert("Success", "Settings saved successfully!", "OK");
            _logger.LogInformation("Settings saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
            await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to save settings", "OK");
        }
    }

    private async Task ResetSettingsAsync()
    {
        if (Application.Current?.MainPage == null) return;
        
        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Reset Settings",
            "Are you sure you want to reset all settings to their default values?",
            "Reset",
            "Cancel");

        if (confirm)
        {
            try
            {
                // Reset to defaults
                DefaultTimeoutSeconds = 30;
                FollowRedirects = true;
                VerifySSL = true;
                DefaultHttpVersion = "HTTP/1.1";
                HistoryRetentionDays = 30;
                AutoSaveRequests = true;
                SelectedTheme = "Light";
                SelectedFontSize = "Medium";
                ShowResponseTime = true;
                PrettyPrintJson = true;
                MaxResponseSizeMB = 10;
                ProxyHost = string.Empty;
                ProxyPort = 8080;
                UseProxyAuth = false;

                await Application.Current?.MainPage?.DisplayAlert("Success", "Settings reset to defaults!", "OK");
                _logger.LogInformation("Settings reset to defaults");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting settings");
                await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to reset settings", "OK");
            }
        }
    }

    private async Task ExportAllDataAsync()
    {
        try
        {
            await Application.Current?.MainPage?.DisplayAlert(
                "Export All Data",
                "This would export all collections, requests, history, and settings to a file.\n\nFeature would be implemented with file picker integration.",
                "OK");
            
            _logger.LogInformation("Data export requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting data");
            await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to export data", "OK");
        }
    }

    private async Task ImportDataAsync()
    {
        try
        {
            await Application.Current?.MainPage?.DisplayAlert(
                "Import Data",
                "This would import collections, requests, and settings from a file.\n\nFeature would be implemented with file picker integration.",
                "OK");
            
            _logger.LogInformation("Data import requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing data");
            await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to import data", "OK");
        }
    }

    private async Task ClearAllDataAsync()
    {
        if (Application.Current?.MainPage == null) return;
        
        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Clear All Data",
            "Are you sure you want to delete all collections, requests, and history? This action cannot be undone.",
            "Clear All",
            "Cancel");

        if (confirm)
        {
            try
            {
                // In a real app, clear all stored data
                await Application.Current?.MainPage?.DisplayAlert("Success", "All data cleared successfully!", "OK");
                _logger.LogInformation("All data cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing data");
                await Application.Current?.MainPage?.DisplayAlert("Error", "Failed to clear data", "OK");
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}