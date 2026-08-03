using Microsoft.Extensions.Logging;
using PosterFly.Services;
using PosterFly.ViewModels;
using PosterFly.Views;

namespace PosterFly;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register HttpClient
		builder.Services.AddHttpClient();

		// Register Services
		builder.Services.AddSingleton<IApiService, ApiService>();
		builder.Services.AddSingleton<IStorageService, StorageService>();
		builder.Services.AddSingleton<IVariableService, VariableService>();

		// Register ViewModels
		builder.Services.AddTransient<RequestsViewModel>();
		builder.Services.AddTransient<CollectionsViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<VariablesViewModel>();

		// Register Views
		builder.Services.AddTransient<RequestsPage>();
		builder.Services.AddTransient<CollectionsPage>();
		builder.Services.AddTransient<HistoryPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<VariablesPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
