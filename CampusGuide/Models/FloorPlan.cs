namespace CampusGuide.Models
{
    /// <summary>
    /// Represents a floor plan for a building floor.
    /// Contains floor plan image data and location mapping.
    /// </summary>
    public class FloorPlan
    {
        /// <summary>
        /// Unique floor plan identifier
        /// </summary>
        public string FloorPlanId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Building name this floor plan belongs to
        /// </summary>
        public string BuildingName { get; set; } = string.Empty;

        /// <summary>
        /// Floor level (G, B1, 1, 2, 3, etc.)
        /// </summary>
        public string FloorLevel { get; set; } = string.Empty;

        /// <summary>
        /// Path to floor plan image resource
        /// Format: "building_name_floor_level.svg"
        /// Example: "floorplans/academic_block_a_1.svg"
        /// </summary>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>
        /// Whether the floor plan image exists in resources
        /// </summary>
        public bool IsAvailable { get; set; } = false;

        /// <summary>
        /// Floor plan image width in pixels
        /// </summary>
        public int ImageWidth { get; set; } = 1200;

        /// <summary>
        /// Floor plan image height in pixels
        /// </summary>
        public int ImageHeight { get; set; } = 800;

        /// <summary>
        /// Aspect ratio (width / height)
        /// </summary>
        public double AspectRatio => ImageWidth / (double)ImageHeight;

        /// <summary>
        /// Locations on this floor with their floor plan coordinates
        /// Key: Location ID, Value: (X, Y) position on floor plan
        /// </summary>
        public Dictionary<string, (double X, double Y)> LocationPositions { get; set; } = new Dictionary<string, (double, double)>();

        /// <summary>
        /// Locations that are on this floor
        /// </summary>
        public List<string> LocationIds { get; set; } = new List<string>();

        /// <summary>
        /// Number of locations on this floor
        /// </summary>
        public int LocationCount => LocationIds.Count;

        /// <summary>
        /// Display name for floor
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Floor plan description or notes
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Create floor plan from building and floor level
        /// </summary>
        public static FloorPlan Create(string buildingName, string floorLevel)
        {
            var floorPlan = new FloorPlan
            {
                BuildingName = buildingName,
                FloorLevel = floorLevel,
                DisplayName = FormatFloorDisplay(floorLevel),
                ImagePath = GenerateImagePath(buildingName, floorLevel)
            };

            return floorPlan;
        }

        /// <summary>
        /// Generate image path from building name and floor level
        /// </summary>
        private static string GenerateImagePath(string buildingName, string floorLevel)
        {
            // Convert to snake_case and lowercase
            var normalizedBuilding = buildingName
                .ToLower()
                .Replace(" ", "_")
                .Replace("-", "_");

            var normalizedFloor = floorLevel
                .ToLower()
                .Replace(" ", "_");

            return $"floorplans/{normalizedBuilding}_{normalizedFloor}.svg";
        }

        /// <summary>
        /// Format floor level for display
        /// </summary>
        private static string FormatFloorDisplay(string floorLevel)
        {
            return floorLevel.ToUpper() switch
            {
                "G" => "Ground Floor",
                var x when x.StartsWith("B") => $"Basement {x.Substring(1)}",
                _ => $"Floor {floorLevel}"
            };
        }

        /// <summary>
        /// Add location to floor plan with position
        /// </summary>
        public void AddLocation(string locationId, double x, double y)
        {
            if (!LocationIds.Contains(locationId))
            {
                LocationIds.Add(locationId);
            }

            LocationPositions[locationId] = (x, y);
        }

        /// <summary>
        /// Remove location from floor plan
        /// </summary>
        public void RemoveLocation(string locationId)
        {
            LocationIds.Remove(locationId);
            LocationPositions.Remove(locationId);
        }

        /// <summary>
        /// Get position of location on floor plan
        /// Returns null if location not found
        /// </summary>
        public (double X, double Y)? GetLocationPosition(string locationId)
        {
            return LocationPositions.ContainsKey(locationId)
                ? LocationPositions[locationId]
                : null;
        }

        /// <summary>
        /// Calculate pixel position from normalized coordinates (0-1 range)
        /// </summary>
        public (int PixelX, int PixelY) GetPixelPosition(double normalizedX, double normalizedY)
        {
            var pixelX = (int)(normalizedX * ImageWidth);
            var pixelY = (int)(normalizedY * ImageHeight);

            return (pixelX, pixelY);
        }

        /// <summary>
        /// Calculate normalized coordinates from pixel position
        /// </summary>
        public (double NormX, double NormY) GetNormalizedPosition(int pixelX, int pixelY)
        {
            var normX = pixelX / (double)ImageWidth;
            var normY = pixelY / (double)ImageHeight;

            // Clamp to 0-1 range
            normX = Math.Clamp(normX, 0, 1);
            normY = Math.Clamp(normY, 0, 1);

            return (normX, normY);
        }

        /// <summary>
        /// Get summary text for floor plan
        /// </summary>
        public string GetSummary()
        {
            return $"{DisplayName} - {LocationCount} location{(LocationCount != 1 ? "s" : "")}";
        }

        /// <summary>
        /// Check if floor plan has valid data
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(BuildingName) &&
                   !string.IsNullOrEmpty(FloorLevel) &&
                   !string.IsNullOrEmpty(ImagePath) &&
                   ImageWidth > 0 &&
                   ImageHeight > 0;
        }
    }
}
