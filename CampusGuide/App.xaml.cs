using Microsoft.Maui.Controls;
using CampusGuide.Services;

namespace CampusGuide
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            System.Diagnostics.Debug.WriteLine("=== CampusGuide CreateWindow called ===");
            try
            {
                var window = new Window(new AppShell());
                System.Diagnostics.Debug.WriteLine("✓ Window and AppShell created");
                return window;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ CreateWindow failed: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"  StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            System.Diagnostics.Debug.WriteLine("=== CampusGuide App OnStart ===");
            try
            {
                SetupExceptionHandlers();
                System.Diagnostics.Debug.WriteLine("✓ Exception handlers set up");
                // Disabled: InitializeFirestoreData();  // Skip Firebase init for testing without config
                System.Diagnostics.Debug.WriteLine("✓ OnStart completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ OnStart failed: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"  StackTrace: {ex.StackTrace}");
                HandleException(ex, "OnStart");
            }
        }

        private void InitializeFirestoreData()
        {
            // Seed Firestore with test data if needed (fire and forget, don't block UI)
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var seeder = ServiceHelper.GetService<FirestoreDataSeeder>();
                    if (seeder != null)
                    {
                        await seeder.SeedDataIfEmptyAsync();
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex, "InitializeFirestoreData");
                }
            });
        }

        private void SetupExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

#if ANDROID
            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
#endif
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            HandleException(ex, "AppDomain");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception, "TaskScheduler");
            e.SetObserved();
        }

#if ANDROID
        private void AndroidEnvironment_UnhandledExceptionRaiser(object sender, Android.Runtime.RaiseThrowableEventArgs e)
        {
            e.Handled = true;
            if (e.Exception != null)
            {
                HandleException(e.Exception, "Android");
            }
        }
#endif

        private void HandleException(Exception ex, string source)
        {
            if (ex == null) return;

            try
            {
                // Log the exception details
                LogException(ex, source);

                // Show user-friendly error message on main thread
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var window = Application.Current?.Windows.FirstOrDefault();
                    if (window?.Page != null)
                    {
                        await window.Page.DisplayAlert("Error", 
                            $"An unexpected error occurred.\n\nDetails: {ex.Message}", 
                            "OK");
                    }
                });
            }
            catch
            {
                // If we can't handle the exception gracefully, at least try to log it
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in exception handler ({source}): {ex}");
            }
        }

        private void LogException(Exception ex, string source)
        {
            string message = $"[{source}] Unhandled Exception: {ex.GetType().Name}\n" +
                           $"Message: {ex.Message}\n" +
                           $"StackTrace: {ex.StackTrace}";

            if (ex.InnerException != null)
            {
                message += $"\nInner Exception: {ex.InnerException.GetType().Name}\n" +
                          $"Inner Message: {ex.InnerException.Message}";
            }

            System.Diagnostics.Debug.WriteLine(message);

#if ANDROID
            Android.Util.Log.Error("CampusGuide", message);
#endif
        }
    }
}