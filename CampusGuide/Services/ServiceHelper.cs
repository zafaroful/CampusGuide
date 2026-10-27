using Microsoft.Extensions.DependencyInjection;

namespace CampusGuide.Services
{
    /// <summary>
    /// Global service provider helper for accessing DI services.
    /// Set during MauiProgram initialization.
    /// </summary>
    public static class ServiceHelper
    {
        public static IServiceProvider? ServiceProvider { get; set; }

        public static T? GetService<T>() where T : class
        {
            return ServiceProvider?.GetService<T>();
        }
    }
}
