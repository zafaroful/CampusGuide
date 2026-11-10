namespace CampusGuide.Models
{
    /// <summary>
    /// Represents a building on campus with location grouping.
    /// Used for filtering and organizing locations by building.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Unique building identifier
        /// </summary>
        public string BuildingId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name of the building
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Number of locations in this building
        /// </summary>
        public int LocationCount { get; set; } = 0;

        /// <summary>
        /// Emoji representing the building type
        /// </summary>
        public string Emoji { get; set; } = "🏢";

        /// <summary>
        /// Short description of the building
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Dominant location type in this building (for coloring)
        /// </summary>
        public string DominantType { get; set; } = string.Empty;

        /// <summary>
        /// List of location IDs within this building
        /// </summary>
        public List<string> LocationIds { get; set; } = new List<string>();

        /// <summary>
        /// Center coordinates of building (average of all locations)
        /// </summary>
        public Coordinates CenterLocation { get; set; } = new Coordinates();

        /// <summary>
        /// Floors in this building (collected from locations)
        /// </summary>
        public List<string> Floors { get; set; } = new List<string>();

        /// <summary>
        /// Create building from location name and locations
        /// </summary>
        public static Building FromLocations(string buildingName, List<Location> locations)
        {
            if (string.IsNullOrEmpty(buildingName) || locations == null || locations.Count == 0)
                return new Building { Name = buildingName ?? "Unknown Building" };

            var building = new Building
            {
                Name = buildingName,
                LocationCount = locations.Count,
                LocationIds = locations.Select(l => l.Id).ToList(),
                Emoji = GetBuildingEmoji(buildingName)
            };

            // Calculate center coordinates
            var avgLat = locations.Average(l => l.Coordinates.Latitude);
            var avgLon = locations.Average(l => l.Coordinates.Longitude);
            building.CenterLocation = new Coordinates { Latitude = avgLat, Longitude = avgLon };

            // Collect unique floors and sort
            var floors = locations
                .Select(l => l.Floor)
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .ToList();

            // Sort floors: G, B1, B2... then 1, 2, 3...
            building.Floors = SortFloors(floors);

            // Determine dominant type
            building.DominantType = locations
                .GroupBy(l => l.Type)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?
                .Key ?? "mixed";

            // Set description
            building.Description = $"{building.LocationCount} location{(building.LocationCount != 1 ? "s" : "")} • {string.Join(", ", building.Floors)}";

            return building;
        }

        /// <summary>
        /// Get emoji for building based on name or dominant type
        /// </summary>
        private static string GetBuildingEmoji(string buildingName)
        {
            var lower = buildingName?.ToLower() ?? "";

            return lower switch
            {
                var x when x.Contains("academic") => "📚",      // Books for academic
                var x when x.Contains("student") => "👥",       // People for student center
                var x when x.Contains("recreation") || x.Contains("sports") => "🏋️",  // Gym
                var x when x.Contains("engineering") => "⚙️",    // Gears for engineering
                var x when x.Contains("medical") || x.Contains("health") => "⚕️",     // Medical
                var x when x.Contains("library") => "📖",       // Books
                var x when x.Contains("cafeteria") || x.Contains("food") => "🍽️",    // Dining
                var x when x.Contains("auditorium") => "🎭",    // Theater
                var x when x.Contains("lab") => "🔬",           // Science
                _ => "🏢"                                        // Generic building
            };
        }

        /// <summary>
        /// Sort floors in logical order (G, B1, B2..., then 1, 2, 3...)
        /// </summary>
        private static List<string> SortFloors(List<string> floors)
        {
            if (floors == null || floors.Count == 0)
                return new List<string>();

            var sorted = new List<string>();

            // Add ground floor first
            if (floors.Contains("G"))
                sorted.Add("G");

            // Add basements in order
            var basements = floors
                .Where(f => f.StartsWith("B"))
                .OrderBy(f => int.TryParse(f.Substring(1), out var num) ? num : 0)
                .ToList();
            sorted.AddRange(basements);

            // Add regular floors in numeric order
            var regularFloors = floors
                .Where(f => !f.StartsWith("B") && f != "G" && int.TryParse(f, out _))
                .OrderBy(f => int.Parse(f))
                .ToList();
            sorted.AddRange(regularFloors);

            // Add any other floors at the end
            var other = floors
                .Where(f => !sorted.Contains(f))
                .OrderBy(f => f)
                .ToList();
            sorted.AddRange(other);

            return sorted;
        }

        /// <summary>
        /// Get color for building based on dominant type
        /// </summary>
        public string GetBuildingColor()
        {
            return DominantType?.ToLower() switch
            {
                "library" => "#0000FF",      // Blue
                "cafeteria" => "#FF8C00",    // Orange
                "sports" => "#008000",       // Green
                "lab" => "#800080",          // Purple
                "auditorium" => "#FF0000",   // Red
                "health" => "#FFC0CB",       // Pink
                _ => "#808080"               // Gray
            };
        }

        /// <summary>
        /// Get display text for building (name and location count)
        /// </summary>
        public string GetDisplayText()
        {
            return $"{Name} ({LocationCount})";
        }
    }
}
