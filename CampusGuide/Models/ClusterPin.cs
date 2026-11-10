namespace CampusGuide.Models
{
    /// <summary>
    /// Represents a cluster of nearby map pins.
    /// Used for grouping multiple locations that are close together.
    /// </summary>
    public class ClusterPin
    {
        /// <summary>
        /// Unique cluster identifier
        /// </summary>
        public string ClusterId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Center location of the cluster (average of all pins)
        /// </summary>
        public Coordinates CenterLocation { get; set; } = new Coordinates();

        /// <summary>
        /// All pins in this cluster
        /// </summary>
        public List<MapPin> Pins { get; set; } = new List<MapPin>();

        /// <summary>
        /// Number of pins in cluster
        /// </summary>
        public int PinCount => Pins.Count;

        /// <summary>
        /// Whether this cluster has multiple pins
        /// </summary>
        public bool IsCluster => PinCount > 1;

        /// <summary>
        /// Radius of cluster in degrees (for zoom calculation)
        /// </summary>
        public double ClusterRadius { get; set; } = 0.01; // ~1km

        /// <summary>
        /// Whether cluster is expanded (individual pins visible)
        /// </summary>
        public bool IsExpanded { get; set; } = false;

        /// <summary>
        /// Dominant location type in cluster
        /// </summary>
        public string DominantType { get; set; } = string.Empty;

        /// <summary>
        /// Create cluster from pins
        /// </summary>
        public void CalculateClusterProperties()
        {
            if (Pins.Count == 0) return;

            // Calculate center point
            var avgLat = Pins.Average(p => p.Coordinates.Latitude);
            var avgLon = Pins.Average(p => p.Coordinates.Longitude);
            CenterLocation = new Coordinates { Latitude = avgLat, Longitude = avgLon };

            // Find dominant type
            DominantType = Pins
                .GroupBy(p => p.Type)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?
                .Key ?? "mixed";

            // Calculate cluster radius based on spread
            if (Pins.Count > 1)
            {
                var maxDistance = Pins.Max(p => 
                    CalculateDistance(CenterLocation.Latitude, CenterLocation.Longitude,
                                    p.Coordinates.Latitude, p.Coordinates.Longitude));
                ClusterRadius = maxDistance / 111.0; // Convert km to degrees
            }
        }

        /// <summary>
        /// Calculate distance between two coordinates (Haversine formula)
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        /// <summary>
        /// Get cluster display text (e.g., "3" for 3 pins)
        /// </summary>
        public string GetClusterBadgeText()
        {
            return IsCluster ? $"{PinCount}" : "•";
        }

        /// <summary>
        /// Get cluster display color based on dominant type
        /// </summary>
        public string GetClusterColor()
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
        /// Get emoji for cluster
        /// </summary>
        public string GetClusterEmoji()
        {
            return DominantType?.ToLower() switch
            {
                "library" => "📚",
                "cafeteria" => "🍽️",
                "sports" => "🏋️",
                "lab" => "🔬",
                "auditorium" => "🎤",
                "health" => "⚕️",
                _ => "📍"
            };
        }
    }
}
