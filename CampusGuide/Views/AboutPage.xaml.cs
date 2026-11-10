using System.Diagnostics;

namespace CampusGuide.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            Debug.WriteLine("AboutPage: Initialized");
        }

        private async void OnLicenseClicked(object? sender, EventArgs e)
        {
            await DisplayAlert("MIT License", 
                "CampusGuide © 2026\n\n" +
                "Permission is hereby granted, free of charge, to any person obtaining a copy of this software " +
                "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, " +
                "and to permit persons to whom the Software is furnished to do so.\n\n" +
                "THE SOFTWARE IS PROVIDED \"AS IS\" WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED.",
                "OK");
            Debug.WriteLine("AboutPage: License displayed");
        }

        private async void OnGitHubClicked(object? sender, EventArgs e)
        {
            try
            {
                await Launcher.Default.OpenAsync(new Uri("https://github.com/zafaroful/CampusGuide"));
                Debug.WriteLine("AboutPage: Opening GitHub repository");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AboutPage: Error opening GitHub - {ex.Message}");
                await DisplayAlert("Error", "Could not open GitHub link. Please visit: https://github.com/zafaroful/CampusGuide", "OK");
            }
        }

        private async void OnFeedbackClicked(object? sender, EventArgs e)
        {
            try
            {
                if (Email.Default.IsComposeSupported)
                {
                    var message = new EmailMessage
                    {
                        Subject = "CampusGuide Feedback - v1.0.0",
                        Body = "Please share your feedback and suggestions for CampusGuide:\n\n",
                        To = new List<string> { "feedback@campusguide.dev" }
                    };

                    await Email.Default.ComposeAsync(message);
                    Debug.WriteLine("AboutPage: Email compose opened");
                }
                else
                {
                    await DisplayAlert("Information", 
                        "Email client not configured on this device.\n\n" +
                        "Please send feedback to: feedback@campusguide.dev", 
                        "OK");
                    Debug.WriteLine("AboutPage: Email not supported");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AboutPage: Error sending feedback - {ex.Message}");
                await DisplayAlert("Error", "Could not open email client. Please email: feedback@campusguide.dev", "OK");
            }
        }
    }
}
