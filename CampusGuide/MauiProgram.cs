using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CampusGuide.Services;
using CampusGuide.ViewModels;
using CampusGuide.ViewModels.Map;
using CampusGuide.ViewModels.Search;
using CampusGuide.Views.Home;
using CampusGuide.Views.Map;
using CampusGuide.Views.Search;
using CampusGuide.Views.Notifications;

namespace CampusGuide
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services for dependency injection
            builder.Services.AddSingleton<IFirestoreService, FirebaseFirestoreService>();
            builder.Services.AddSingleton<IQRCodeScannerService, ZXingQRCodeScannerService>();
            builder.Services.AddSingleton<IFavoritesService, PreferencesFavoritesService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();

            // Register ViewModels
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<CampusMapViewModel>();
            builder.Services.AddSingleton<SearchViewModel>();
            builder.Services.AddSingleton<NotificationsViewModel>();

            // Register Pages
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<CampusMapPage>();
            builder.Services.AddSingleton<SearchPage>();
            builder.Services.AddSingleton<NotificationsPage>();
            builder.Services.AddSingleton<QRScannerPage>();

            var mauiApp = builder.Build();

            // Store service provider for later access via ServiceHelper
            ServiceHelper.ServiceProvider = mauiApp.Services;

            return mauiApp;
        }
    }
}
