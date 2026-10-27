using Microsoft.Maui.Controls;

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
            return new Window(new AppShell());
        }

        protected override void OnStart()
        {
            base.OnStart();
            SetupExceptionHandlers();
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