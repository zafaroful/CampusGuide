using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using CampusGuide.Services;
using CampusGuide.ViewModels;
using CampusGuide.ViewModels.Map;
using CampusGuide.ViewModels.Search;
using CampusGuide.Views.Home;
using CampusGuide.Views.Map;
using CampusGuide.Views.Search;
using CampusGuide.Views.Notifications;
using CampusGuide.Views.Favorites;
using CampusGuide.Views.Events;
using CampusGuide.Views;

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
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services for dependency injection
#if ANDROID
            builder.Services.AddSingleton<IFirestoreService, FirebaseFirestoreService>();
#else
            builder.Services.AddSingleton<IFirestoreService>(s => new NullFirestoreService());
#endif
            builder.Services.AddSingleton<IQRCodeScannerService, ZXingQRCodeScannerService>();
            builder.Services.AddSingleton<IFavoritesService, PreferencesFavoritesService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<FirestoreDataSeeder>();

            // Register ViewModels
            builder.Services.AddSingleton<HomeViewModel>();
            builder.Services.AddSingleton<CampusMapViewModel>();
            builder.Services.AddSingleton<SearchViewModel>();
            builder.Services.AddSingleton<NotificationsViewModel>();
            builder.Services.AddSingleton<FavoritesViewModel>();
            builder.Services.AddSingleton<EventsViewModel>();
            builder.Services.AddSingleton<FloorPlanViewModel>();

            // Register Pages
            builder.Services.AddSingleton<HomePage>();
            builder.Services.AddSingleton<CampusMapPage>();
            builder.Services.AddSingleton<SearchPage>();
            builder.Services.AddSingleton<NotificationsPage>();
            builder.Services.AddSingleton<QRScannerPage>();
            builder.Services.AddSingleton<FavoritesPage>();
            builder.Services.AddSingleton<EventsPage>();
            builder.Services.AddSingleton<EventDetailsPage>();
            builder.Services.AddSingleton<FloorPlanPage>();
            builder.Services.AddSingleton<AboutPage>();
            builder.Services.AddSingleton<EventDetailsPage>();
            builder.Services.AddSingleton<FloorPlanPage>();

            var mauiApp = builder.Build();

            // Store service provider for later access via ServiceHelper
            ServiceHelper.ServiceProvider = mauiApp.Services;

            return mauiApp;
        }
    }
}
