using Microsoft.Extensions.Logging;
using TravelGuideApp.Services;
using TravelGuideApp.Database;
using TravelGuideApp.ViewModels;
using System;
using System.Net.Http;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Plugin.LocalNotification;

namespace TravelGuideApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseMauiMaps()
				.UseBarcodeReader()
				.UseLocalNotification()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Services
		builder.Services.AddSingleton<SQLiteService>();
		builder.Services.AddSingleton<LocationService>();
			builder.Services.AddSingleton<GeofenceService>();
			builder.Services.AddSingleton<NarrationService>();
			builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(ApiSettings.BaseUrl) });
			builder.Services.AddSingleton<SyncService>();
			builder.Services.AddSingleton<OfflineContentService>();

		// ViewModels
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<MapViewModel>();
		builder.Services.AddTransient<ToursViewModel>();
		builder.Services.AddTransient<AudioGuideViewModel>();
		builder.Services.AddTransient<QRScanViewModel>();
		builder.Services.AddTransient<POIViewModel>();
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<TourDetailViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();
		builder.Services.AddTransient<FavoritesViewModel>();
		builder.Services.AddTransient<TourReviewViewModel>();
		
		// Views
		builder.Services.AddTransient<Views.HomePage>();
		builder.Services.AddTransient<Views.MapPage>();
		builder.Services.AddTransient<Views.ToursPage>();
		builder.Services.AddTransient<Views.AudioGuidePage>();
		builder.Services.AddTransient<Views.QRScanPage>();
		builder.Services.AddTransient<Views.POIDetailPage>();
		builder.Services.AddTransient<Views.LoginPage>();
		builder.Services.AddTransient<Views.RegisterPage>();
		builder.Services.AddTransient<Views.TourDetailPage>();
		builder.Services.AddTransient<Views.ProfilePage>();
		builder.Services.AddTransient<Views.HistoryPage>();
		builder.Services.AddTransient<Views.FavoritesPage>();
		builder.Services.AddTransient<Views.TourReviewPage>();

		return builder.Build();
	}
}
