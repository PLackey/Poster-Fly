using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using PosterFly.Models;
using PosterFly.Services;
using Environment = PosterFly.Models.Environment;

namespace PosterFly.ViewModels;

public class RequestsViewModel : INotifyPropertyChanged
{
    private readonly IApiService _apiService;
    private readonly IStorageService _storageService;
    private readonly IVariableService _variableService;
    private readonly ILogger<RequestsViewModel> _logger;
    
    private ApiRequest _currentRequest = new();
    private ApiResponse? _lastResponse;
    private bool _isLoading;
    private Environment? _activeEnvironment;

    public RequestsViewModel(IApiService apiService, IStorageService storageService, IVariableService variableService, ILogger<RequestsViewModel> logger)
    {
        _apiService = apiService;
        _storageService = storageService;
        _variableService = variableService;
        _logger = logger;
        
        CurrentRequest = new ApiRequest { Name = "New Request" };
        
        SendRequestCommand = new Command(async () => await SendRequestAsync(), () => !IsLoading);
        SaveRequestCommand = new Command(async () => await SaveRequestAsync());
        AddHeaderCommand = new Command(() => AddHeader());
        
        LoadInitialData();
    }

    public ApiRequest CurrentRequest
    {
        get => _currentRequest;
        set
        {
            _currentRequest = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHttpRequest));
            OnPropertyChanged(nameof(IsGraphQLRequest));
            OnPropertyChanged(nameof(IsGrpcRequest));
            OnPropertyChanged(nameof(ShowBodySection));
        }
    }

    public ApiResponse? LastResponse
    {
        get => _lastResponse;
        set
        {
            _lastResponse = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasResponse));
            OnPropertyChanged(nameof(ResponseHeadersList));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
            ((Command)SendRequestCommand).ChangeCanExecute();
        }
    }

    public bool HasResponse => LastResponse != null;
    
    public bool IsHttpRequest => CurrentRequest.Type == RequestType.HTTP;
    public bool IsGraphQLRequest => CurrentRequest.Type == RequestType.GraphQL;
    public bool IsGrpcRequest => CurrentRequest.Type == RequestType.GRPC;
    
    public bool ShowBodySection => IsHttpRequest && 
        (CurrentRequest.Method == PosterFly.Models.HttpMethod.POST || 
         CurrentRequest.Method == PosterFly.Models.HttpMethod.PUT || 
         CurrentRequest.Method == PosterFly.Models.HttpMethod.PATCH);

    public ObservableCollection<KeyValuePair<string, string>> ResponseHeadersList =>
        new(LastResponse?.Headers ?? new Dictionary<string, string>());

    public List<string> RequestTypes => Enum.GetNames(typeof(RequestType)).ToList();
    public List<string> HttpMethods => Enum.GetNames(typeof(PosterFly.Models.HttpMethod)).ToList();
    public List<string> BodyTypes => new() { "JSON", "XML", "Text", "Form", "Binary" };

    public string SelectedRequestType
    {
        get => CurrentRequest.Type.ToString();
        set
        {
            if (Enum.TryParse<RequestType>(value, out var requestType))
            {
                CurrentRequest.Type = requestType;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsHttpRequest));
                OnPropertyChanged(nameof(IsGraphQLRequest));
                OnPropertyChanged(nameof(IsGrpcRequest));
                OnPropertyChanged(nameof(ShowBodySection));
            }
        }
    }

    public string SelectedHttpMethod
    {
        get => CurrentRequest.Method.ToString();
        set
        {
            if (Enum.TryParse<PosterFly.Models.HttpMethod>(value, out var httpMethod))
            {
                CurrentRequest.Method = httpMethod;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowBodySection));
            }
        }
    }

    public ICommand SendRequestCommand { get; }
    public ICommand SaveRequestCommand { get; }
    public ICommand AddHeaderCommand { get; }

    private async Task SendRequestAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentRequest.Url))
        {
            var mainPage = Application.Current?.MainPage;
            if (mainPage != null)
                await mainPage.DisplayAlert("Error", "Please enter a URL", "OK");
            return;
        }

        IsLoading = true;
        LastResponse = null;

        try
        {
            // Load active environment for variable resolution
            _activeEnvironment = await _variableService.GetActiveEnvironmentAsync();
            
            // Create a resolved copy of the request with variables substituted
            var resolvedRequest = await ResolveVariablesInRequestAsync(CurrentRequest);
            
            ApiResponse response = resolvedRequest.Type switch
            {
                RequestType.HTTP => await _apiService.SendHttpRequestAsync(resolvedRequest),
                RequestType.GraphQL => await _apiService.SendGraphQLRequestAsync(resolvedRequest),
                RequestType.GRPC => await _apiService.SendGrpcRequestAsync(resolvedRequest),
                _ => throw new NotSupportedException($"Request type {resolvedRequest.Type} is not supported")
            };

            LastResponse = response;
            
            // Save to history (save original request, not resolved)
            await _storageService.SaveRequestHistoryAsync(CurrentRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending request");
            var mainPage = Application.Current?.MainPage;
            if (mainPage != null)
                await mainPage.DisplayAlert("Error", $"Request failed: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task<ApiRequest> ResolveVariablesInRequestAsync(ApiRequest originalRequest)
    {
        var resolvedRequest = new ApiRequest
        {
            Id = originalRequest.Id,
            Name = originalRequest.Name,
            Type = originalRequest.Type,
            Method = originalRequest.Method,
            UseSSL = originalRequest.UseSSL,
            UseHttp2 = originalRequest.UseHttp2,
            TimeoutSeconds = originalRequest.TimeoutSeconds,
            BodyType = originalRequest.BodyType,
            CollectionId = originalRequest.CollectionId,
            FolderPath = originalRequest.FolderPath,
            CreatedAt = originalRequest.CreatedAt,
            LastModified = originalRequest.LastModified
        };

        // Resolve variables in URL
        resolvedRequest.Url = _variableService.ResolveVariables(originalRequest.Url, _activeEnvironment, originalRequest.CollectionId);

        // Resolve variables in headers
        resolvedRequest.Headers = new Dictionary<string, string>();
        foreach (var header in originalRequest.Headers)
        {
            var resolvedKey = _variableService.ResolveVariables(header.Key, _activeEnvironment, originalRequest.CollectionId);
            var resolvedValue = _variableService.ResolveVariables(header.Value, _activeEnvironment, originalRequest.CollectionId);
            resolvedRequest.Headers[resolvedKey] = resolvedValue;
        }

        // Resolve variables in body
        resolvedRequest.Body = _variableService.ResolveVariables(originalRequest.Body, _activeEnvironment, originalRequest.CollectionId);

        // Resolve variables in GraphQL specific fields
        resolvedRequest.GraphQLQuery = _variableService.ResolveVariables(originalRequest.GraphQLQuery ?? string.Empty, _activeEnvironment, originalRequest.CollectionId);
        resolvedRequest.GraphQLVariables = _variableService.ResolveVariables(originalRequest.GraphQLVariables ?? string.Empty, _activeEnvironment, originalRequest.CollectionId);

        // Resolve variables in gRPC specific fields
        resolvedRequest.GrpcService = _variableService.ResolveVariables(originalRequest.GrpcService ?? string.Empty, _activeEnvironment, originalRequest.CollectionId);
        resolvedRequest.GrpcMethod = _variableService.ResolveVariables(originalRequest.GrpcMethod ?? string.Empty, _activeEnvironment, originalRequest.CollectionId);
        resolvedRequest.ProtoFile = _variableService.ResolveVariables(originalRequest.ProtoFile ?? string.Empty, _activeEnvironment, originalRequest.CollectionId);

        return Task.FromResult(resolvedRequest);
    }

    public async Task<List<string>> GetVariableAutoCompleteAsync(string prefix)
    {
        try
        {
            return await _variableService.GetAutoCompleteVariablesAsync(prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting variable autocomplete");
            return new List<string>();
        }
    }

    private async Task SaveRequestAsync()
    {
        if (LastResponse == null)
            return;

        var mainPage = Application.Current?.MainPage;
        if (mainPage == null) return;

        var name = await mainPage.DisplayPromptAsync(
            "Save Request", 
            "Enter a name for this request:", 
            "Save", 
            "Cancel", 
            CurrentRequest.Name);

        if (!string.IsNullOrWhiteSpace(name))
        {
            CurrentRequest.Name = name;
            
            // For now, create a simple collection if none exists
            var collections = await _storageService.LoadCollectionsAsync();
            var defaultCollection = collections.FirstOrDefault(c => c.Name == "Default") 
                ?? new Collection { Name = "Default", Description = "Default collection" };
            
            CurrentRequest.CollectionId = defaultCollection.Id.ToString();
            defaultCollection.Requests.Add(CurrentRequest);
            
            await _storageService.SaveCollectionAsync(defaultCollection);
            await mainPage.DisplayAlert("Success", "Request saved successfully!", "OK");
        }
    }

    private void AddHeader()
    {
        CurrentRequest.Headers[""] = "";
        OnPropertyChanged(nameof(CurrentRequest));
    }

    private void LoadInitialData()
    {
        // Initialize with common headers
        CurrentRequest.Headers["Content-Type"] = "application/json";
        CurrentRequest.Headers["User-Agent"] = "PosterFly/1.0";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}