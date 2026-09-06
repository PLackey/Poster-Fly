using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using PosterFly.Models;
using PosterFly.Services;

namespace PosterFly.ViewModels;

public class CollectionsViewModel : INotifyPropertyChanged
{
    private readonly IStorageService _storageService;
    private readonly ILogger<CollectionsViewModel> _logger;
    
    private ObservableCollection<Collection> _collections = new();
    private bool _isLoading;

    public CollectionsViewModel(IStorageService storageService, ILogger<CollectionsViewModel> logger)
    {
        _storageService = storageService;
        _logger = logger;
        
        NewCollectionCommand = new Command(async () => await CreateNewCollectionAsync());
        OpenCollectionCommand = new Command<Collection>(async (collection) => await OpenCollectionAsync(collection));
        DeleteCollectionCommand = new Command<Collection>(async (collection) => await DeleteCollectionAsync(collection));
        ExportCollectionCommand = new Command<Collection>(async (collection) => await ExportCollectionAsync(collection));
        ImportCollectionCommand = new Command(async () => await ImportCollectionAsync());
        
        // Fire and forget - we want this to run in the background
        _ = Task.Run(async () => await LoadCollectionsAsync());
    }

    public ObservableCollection<Collection> Collections
    {
        get => _collections;
        set
        {
            _collections = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNoCollections));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool HasNoCollections => !Collections.Any();

    public ICommand NewCollectionCommand { get; }
    public ICommand OpenCollectionCommand { get; }
    public ICommand DeleteCollectionCommand { get; }
    public ICommand ExportCollectionCommand { get; }
    public ICommand ImportCollectionCommand { get; }

    private async Task LoadCollectionsAsync()
    {
        try
        {
            IsLoading = true;
            var collections = await _storageService.LoadCollectionsAsync();
            Collections.Clear();
            foreach (var collection in collections.OrderBy(c => c.Name))
            {
                Collections.Add(collection);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading collections");
            var mainPage = Application.Current?.MainPage;
            if (mainPage != null)
                await mainPage.DisplayAlert("Error", "Failed to load collections", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateNewCollectionAsync()
    {
        var mainPage = Application.Current?.MainPage;
        if (mainPage == null) return;

        var name = await mainPage.DisplayPromptAsync(
            "New Collection",
            "Enter collection name:",
            "Create",
            "Cancel",
            "My Collection");

        if (!string.IsNullOrWhiteSpace(name))
        {
            var description = await mainPage.DisplayPromptAsync(
                "Collection Description",
                "Enter description (optional):",
                "Create",
                "Skip",
                "");

            try
            {
                var collection = new Collection
                {
                    Name = name,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description
                };

                await _storageService.SaveCollectionAsync(collection);
                Collections.Add(collection);
                OnPropertyChanged(nameof(HasNoCollections));

                await mainPage.DisplayAlert("Success", "Collection created successfully!", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating collection");
                await mainPage.DisplayAlert("Error", "Failed to create collection", "OK");
            }
        }
    }

    private async Task OpenCollectionAsync(Collection? collection)
    {
        if (collection == null) return;
        
        var mainPage = Application.Current?.MainPage;
        if (mainPage == null) return;

        // Navigate to collection detail page
        // For now, just show the requests count
        var requestCount = collection.Requests?.Count ?? 0;
        var folderCount = collection.Folders?.Count ?? 0;
        
        await mainPage.DisplayAlert(
            collection.Name,
            $"Requests: {requestCount}\nFolders: {folderCount}\n\nCollection details view would open here.",
            "OK");
    }

    private async Task DeleteCollectionAsync(Collection? collection)
    {
        if (collection == null) return;
        
        var mainPage = Application.Current?.MainPage;
        if (mainPage == null) return;

        var confirm = await mainPage.DisplayAlert(
            "Delete Collection",
            $"Are you sure you want to delete '{collection.Name}'? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (confirm)
        {
            try
            {
                await _storageService.DeleteCollectionAsync(collection.Id);
                Collections.Remove(collection);
                OnPropertyChanged(nameof(HasNoCollections));

                await mainPage.DisplayAlert("Success", "Collection deleted successfully!", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting collection");
                await mainPage.DisplayAlert("Error", "Failed to delete collection", "OK");
            }
        }
    }

    private async Task ExportCollectionAsync(Collection? collection)
    {
        if (collection == null) return;
        
        var mainPage = Application.Current?.MainPage;
        if (mainPage == null) return;

        var format = await mainPage.DisplayActionSheet(
            "Export Format",
            "Cancel",
            null,
            "JSON",
            "Postman");

        if (format != "Cancel" && format != null)
        {
            try
            {
                var exportData = await _storageService.ExportCollectionAsync(collection, format.ToLower());
                
                // In a real app, you'd use the file picker or share dialog
                // For now, just show a preview
                await mainPage.DisplayAlert(
                    "Export Ready",
                    $"Collection exported as {format}.\n\nPreview (first 200 chars):\n{exportData.Substring(0, Math.Min(200, exportData.Length))}...",
                    "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting collection");
                await mainPage.DisplayAlert("Error", "Failed to export collection", "OK");
            }
        }
    }

    private async Task ImportCollectionAsync()
    {
        var mainPage = Application.Current?.MainPage;
        if (mainPage == null) return;

        // In a real app, you'd use a file picker
        // For now, let user paste JSON
        var jsonData = await mainPage.DisplayPromptAsync(
            "Import Collection",
            "Paste collection JSON data:",
            "Import",
            "Cancel",
            "",
            -1,
            Microsoft.Maui.Keyboard.Default);

        if (!string.IsNullOrWhiteSpace(jsonData))
        {
            try
            {
                var collection = await _storageService.ImportCollectionAsync(jsonData);
                if (collection != null)
                {
                    await _storageService.SaveCollectionAsync(collection);
                    Collections.Add(collection);
                    OnPropertyChanged(nameof(HasNoCollections));

                    await mainPage.DisplayAlert("Success", "Collection imported successfully!", "OK");
                }
                else
                {
                    await mainPage.DisplayAlert("Error", "Invalid collection data", "OK");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing collection");
                await mainPage.DisplayAlert("Error", "Failed to import collection", "OK");
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
