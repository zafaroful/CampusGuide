namespace CampusGuide.Models
{
    /// <summary>
    /// Represents a map pin for a location on the native map view.
    /// Wraps location data for display on MAUI Maps.
    /// </summary>
    public class MapPin
    {
        /// <summary>
        /// Unique identifier matching Location.Id for navigation
        /// </summary>
        public string LocationId { get; set; } = string.Empty;

        /// <summary>
        /// Display name shown on map pin
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Pin subtitle (type/category)
        /// </summary>
        public string Subtitle { get; set; } = string.Empty;

        /// <summary>
        /// Location coordinates
        /// </summary>
        public Coordinates Coordinates { get; set; } = new Coordinates();

        /// <summary>
        /// Location type for coloring pins (library, cafeteria, sports, etc.)
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Building name for additional info
        /// </summary>
        public string Building { get; set; } = string.Empty;

        /// <summary>
        /// Floor level
        /// </summary>
        public string Floor { get; set; } = string.Empty;

        /// <summary>
        /// Pin color based on location type (as hex string or color name)
        /// </summary>
        public string PinColor { get; set; } = "#0000FF"; // Default blue

        /// <summary>
        /// Flag for visibility in filtered view
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Create a map pin from a Location model
        /// </summary>
        public static MapPin FromLocation(Location location)
        {
            return new MapPin
            {
                LocationId = location.Id,
                Title = location.Name,
                Subtitle = location.Type,
                Coordinates = location.Coordinates,
                Type = location.Type,
                Building = location.Building,
                Floor = location.Floor,
                PinColor = GetColorForType(location.Type),
                IsVisible = location.IsVisible
            };
        }

        /// <summary>
        /// Get pin color based on location type (returns hex color string)
        /// </summary>
        private static string GetColorForType(string type)
        {
            return type?.ToLower() switch
            {
                "library" => "#0000FF",      // Blue
                "cafeteria" => "#FF8C00",    // Dark Orange
                "sports" => "#008000",       // Green
                "lab" => "#800080",          // Purple
                "auditorium" => "#FF0000",   // Red
                "health" => "#FFC0CB",       // Pink
                _ => "#808080"               // Gray
            };
        }
    }
}
