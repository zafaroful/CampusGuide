namespace CampusGuide
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute("location-details", typeof(Views.Map.LocationDetailsPage));
            Routing.RegisterRoute("event-details", typeof(Views.Events.EventDetailsPage));
        }
    }
}
