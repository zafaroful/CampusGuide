using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using System;

namespace CampusGuide
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        const string TAG = "MainActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Log.Debug(TAG, "MainActivity.OnCreate() - base.OnCreate() completed successfully");

                try
                {
                    // Initialize Firebase (Android)
                    Firebase.FirebaseApp.InitializeApp(this);
                    Log.Debug(TAG, "Firebase initialized successfully.");
                }
                catch (Java.Lang.Exception firebaseEx)
                {
                    Log.Error(TAG, $"Firebase initialization failed with Java exception: {firebaseEx}");
                    // Continue anyway - app can work without Firebase
                }
                catch (System.Exception ex)
                {
                    Log.Error(TAG, $"Firebase initialization failed: {ex.GetType().Name} - {ex.Message}");
                    // Continue anyway - app can work without Firebase
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, $"CRITICAL: Unexpected error in MainActivity.OnCreate(): {ex.GetType().Name}");
                Log.Error(TAG, $"Message: {ex.Message}");
                Log.Error(TAG, $"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Log.Error(TAG, $"Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }

                // Re-throw to let the runtime handle it
                throw;
            }
        }

        protected override void OnStart()
        {
            try
            {
                base.OnStart();
                Log.Debug(TAG, "MainActivity.OnStart()");
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, $"Error in OnStart: {ex.Message}");
                throw;
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                Log.Debug(TAG, "MainActivity.OnResume()");
            }
            catch (System.Exception ex)
            {
                Log.Error(TAG, $"Error in OnResume: {ex.Message}");
                throw;
            }
        }
    }
}
