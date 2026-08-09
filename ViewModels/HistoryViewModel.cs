using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using PosterFly.Models;
using PosterFly.Services;

namespace PosterFly.ViewModels;

public class HistoryViewModel : INotifyPropertyChanged
{
    private readonly IStorageService _storageService;
    private readonly ILogger<HistoryViewModel> _logger;
    
    private ObservableCollection<ApiRequest> _recentRequests = new();
    private bool _isLoading;

    public HistoryViewModel(IStorageService storageService, ILogger<HistoryViewModel> logger)
    {
        _storageService = storageService;
        _logger = logger;
        
        ReuseRequestCommand = new Command<ApiRequest>(async (request) => await ReuseRequestAsync(request));
        SaveFromHistoryCommand = new Command<ApiRequest>(async (request) => await SaveFromHistoryAsync(request));
        DeleteFromHistoryCommand = new Command<ApiRequest>(async (request) => await DeleteFromHistoryAsync(request));
        ClearHistoryCommand = new Command(async () => await ClearHistoryAsync());
        
        // Fire and forget - we want this to run in the background
        _ = Task.Run(async () => await LoadHistoryAsync());
    }

    public ObservableCollection<ApiRequest> RecentRequests
    {
        get => _recentRequests;
        set
        {
            _recentRequests = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNoHistory));
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

    public bool HasNoHistory => !RecentRequests.Any();

    public ICommand ReuseRequestCommand { get; }
    public ICommand SaveFromHistoryCommand { get; }
    public ICommand DeleteFromHistoryCommand { get; }
    public ICommand ClearHistoryCommand { get; }

    private async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            var requests = await _storageService.LoadRecentRequestsAsync(50); // Load last 50 requests
            
            RecentRequests.Clear();
            foreach (var request in requests.OrderByDescending(r => r.CreatedAt))
            {
                RecentRequests.Add(request);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading request history");
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load request history", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ReuseRequestAsync(ApiRequest? request)
    {
        if (request == null || Application.Current?.MainPage == null) return;

        // Navigate to the requests page with this request loaded
        // For now, just show a message
        await Application.Current.MainPage.DisplayAlert(
            "Reuse Request",
            $"Would navigate to Requests tab with:\n\n{request.Method} {request.Url}\n\nThis would be implemented with navigation service.",
            "OK");
    }

    private async Task SaveFromHistoryAsync(ApiRequest? request)
    {
        if (request == null || Application.Current?.MainPage == null) return;

        var name = await Application.Current.MainPage.DisplayPromptAsync(
            "Save Request",
            "Enter a name for this request:",
            "Save",
            "Cancel",
            request.Name);

        if (!string.IsNullOrWhiteSpace(name))
        {
            try
            {
                // Create a new request based on the historical one
                var newRequest = new ApiRequest
                {
                    Name = name,
                    Url = request.Url,
                    Type = request.Type,
                    Method = request.Method,
                    Headers = new Dictionary<string, string>(request.Headers ?? new Dictionary<string, string>()),
                    Body = request.Body,
                    BodyType = request.BodyType,
                    UseSSL = request.UseSSL,
                    UseHttp2 = request.UseHttp2,
                    GraphQLQuery = request.GraphQLQuery,
                    GraphQLVariables = request.GraphQLVariables,
                    GrpcService = request.GrpcService,
                    GrpcMethod = request.GrpcMethod,
                    ProtoFile = request.ProtoFile
                };

                // Save to default collection
                var collections = await _storageService.LoadCollectionsAsync();
                var defaultCollection = collections.FirstOrDefault(c => c.Name == "Default")
                    ?? new Collection { Name = "Default", Description = "Default collection" };

                newRequest.CollectionId = defaultCollection.Id.ToString();
                defaultCollection.Requests?.Add(newRequest);

                await _storageService.SaveCollectionAsync(defaultCollection);
                await Application.Current.MainPage.DisplayAlert("Success", "Request saved to collection!", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving request from history");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to save request", "OK");
            }
        }
    }

    private async Task DeleteFromHistoryAsync(ApiRequest? request)
    {
        if (request == null || Application.Current?.MainPage == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Delete from History",
            $"Remove '{request.Name}' from history?",
            "Delete",
            "Cancel");

        if (confirm)
        {
            try
            {
                RecentRequests.Remove(request);
                OnPropertyChanged(nameof(HasNoHistory));
                
                // Note: In a real implementation, you'd also delete the file from storage
                await Application.Current.MainPage.DisplayAlert("Success", "Request removed from history", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting from history");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete from history", "OK");
            }
        }
    }

    private async Task ClearHistoryAsync()
    {
        if (Application.Current?.MainPage == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Clear History",
            "Are you sure you want to clear all request history? This action cannot be undone.",
            "Clear All",
            "Cancel");

        if (confirm)
        {
            try
            {
                RecentRequests.Clear();
                OnPropertyChanged(nameof(HasNoHistory));
                
                // Note: In a real implementation, you'd clear the history files from storage
                await Application.Current.MainPage.DisplayAlert("Success", "History cleared successfully!", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing history");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to clear history", "OK");
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}