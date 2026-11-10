using CampusGuide.Models;
using System.Diagnostics;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.Services
{
    /// <summary>
    /// Service for managing floor plans and floor plan operations.
    /// Handles floor plan retrieval, location mapping, and floor filtering.
    /// </summary>
    public class FloorPlanService
    {
        /// <summary>
        /// Base path for floor plan images in resources
        /// </summary>
        private const string FloorPlanResourceBase = "floorplans";

        /// <summary>
        /// Cache for floor plans
        /// </summary>
        private Dictionary<string, FloorPlan> _floorPlanCache = new Dictionary<string, FloorPlan>();

        /// <summary>
        /// Get all floor plans for a building
        /// </summary>
        public List<FloorPlan> GetFloorPlansForBuilding(string buildingName, List<LocationModel> locations)
        {
            if (string.IsNullOrEmpty(buildingName) || locations == null)
            {
                Debug.WriteLine("FloorPlanService: Invalid building name or locations");
                return new List<FloorPlan>();
            }

            // Get all unique floors in building
            var floorsInBuilding = locations
                .Where(l => l.Building.Equals(buildingName, StringComparison.OrdinalIgnoreCase))
                .Select(l => l.Floor)
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .OrderBy(f => SortFloorOrder(f))
                .ToList();

            Debug.WriteLine($"FloorPlanService: Found {floorsInBuilding.Count} floors in building '{buildingName}'");

            var floorPlans = new List<FloorPlan>();

            foreach (var floor in floorsInBuilding)
            {
                var floorPlan = GetOrCreateFloorPlan(buildingName, floor, locations);
                floorPlans.Add(floorPlan);
            }

            return floorPlans;
        }

        /// <summary>
        /// Get floor plan for specific building floor
        /// </summary>
        public FloorPlan? GetFloorPlan(string buildingName, string floorLevel, List<LocationModel> locations)
        {
            if (string.IsNullOrEmpty(buildingName) || string.IsNullOrEmpty(floorLevel))
                return null;

            var cacheKey = $"{buildingName}_{floorLevel}";

            if (_floorPlanCache.ContainsKey(cacheKey))
            {
                Debug.WriteLine($"FloorPlanService: Retrieved floor plan from cache: {cacheKey}");
                return _floorPlanCache[cacheKey];
            }

            var floorPlan = GetOrCreateFloorPlan(buildingName, floorLevel, locations);
            _floorPlanCache[cacheKey] = floorPlan;

            return floorPlan;
        }

        /// <summary>
        /// Get or create floor plan for building and floor
        /// </summary>
        private FloorPlan GetOrCreateFloorPlan(string buildingName, string floorLevel, List<LocationModel> locations)
        {
            var floorPlan = FloorPlan.Create(buildingName, floorLevel);

            // Get locations on this floor in this building
            var locationsOnFloor = locations
                .Where(l => l.Building.Equals(buildingName, StringComparison.OrdinalIgnoreCase) &&
                           l.Floor.Equals(floorLevel, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Debug.WriteLine($"FloorPlanService: Floor plan '{buildingName} {floorLevel}' has {locationsOnFloor.Count} locations");

            // Add locations to floor plan
            foreach (var location in locationsOnFloor)
            {
                // Generate estimated position on floor plan
                // In real implementation, this would come from database or floor plan mapping
                var position = GenerateLocationPosition(location);
                floorPlan.AddLocation(location.Id, position.X, position.Y);
            }

            // Check if floor plan image exists (simplified - assume exists for now)
            floorPlan.IsAvailable = CheckFloorPlanImageExists(floorPlan.ImagePath);

            return floorPlan;
        }

        /// <summary>
        /// Check if floor plan image exists in resources
        /// </summary>
        private bool CheckFloorPlanImageExists(string imagePath)
        {
            try
            {
                // In MAUI, embedded resources are loaded via ImageSource.FromFile()
                // For now, we'll check if the resource would be available
                // MAUI will handle the actual resource loading at runtime

                // For SVG resources, check if the file exists in the project Resources/FloorPlans folder
                var fileName = Path.GetFileName(imagePath);
                var resourceFile = Path.Combine(AppContext.BaseDirectory, "Resources", "FloorPlans", fileName);

                // Also try to load it as a MAUI asset to verify availability
                // In development/debug, assets might be in the project directory
                var projectResourceFile = Path.Combine(
                    Path.GetDirectoryName(AppContext.BaseDirectory) ?? "",
                    "..", "..", "..", "..",
                    "Resources", "FloorPlans", fileName);

                var exists = File.Exists(resourceFile) || File.Exists(projectResourceFile);

                Debug.WriteLine($"FloorPlanService: Checking floor plan image: {imagePath} - {(exists ? "EXISTS" : "NOT FOUND")}");
                return exists;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FloorPlanService: Error checking floor plan image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generate estimated location position on floor plan
        /// In a real system, this would come from a database or mapping configuration
        /// </summary>
        private (double X, double Y) GenerateLocationPosition(LocationModel location)
        {
            // Use pseudo-random positioning based on location ID hash
            // In production, location coordinates would be mapped from lat/lon or stored explicitly

            var hash = location.Id.GetHashCode();
            var random = new Random(Math.Abs(hash));

            // Generate position in 0-1 normalized range (with margins)
            var x = 0.2 + (random.NextDouble() * 0.6); // 0.2 to 0.8
            var y = 0.2 + (random.NextDouble() * 0.6); // 0.2 to 0.8

            Debug.WriteLine($"FloorPlanService: Generated position for {location.Name}: ({x:F3}, {y:F3})");

            return (x, y);
        }

        /// <summary>
        /// Get locations for specific floor
        /// </summary>
        public List<LocationModel> GetLocationsOnFloor(List<LocationModel> locations, string buildingName, string floorLevel)
        {
            if (string.IsNullOrEmpty(buildingName) || string.IsNullOrEmpty(floorLevel))
                return new List<LocationModel>();

            var filtered = locations
                .Where(l => l.Building.Equals(buildingName, StringComparison.OrdinalIgnoreCase) &&
                           l.Floor.Equals(floorLevel, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Debug.WriteLine($"FloorPlanService: Found {filtered.Count} locations on {buildingName} {floorLevel}");
            return filtered;
        }

        /// <summary>
        /// Clear floor plan cache
        /// </summary>
        public void ClearCache()
        {
            _floorPlanCache.Clear();
            Debug.WriteLine("FloorPlanService: Cache cleared");
        }

        /// <summary>
        /// Get sort order for floor (used for sorting)
        /// Lower values sort first
        /// </summary>
        private int SortFloorOrder(string floor)
        {
            if (floor == "G")
                return 0;

            if (floor.StartsWith("B"))
            {
                if (int.TryParse(floor.Substring(1), out var basementNum))
                    return -basementNum; // B1 = -1, B2 = -2, etc.
            }

            if (int.TryParse(floor, out var floorNum))
                return floorNum * 100; // Regular floors: 1 = 100, 2 = 200, etc.

            return int.MaxValue; // Unknown floors sort last
        }

        /// <summary>
        /// Get floor plan statistics
        /// </summary>
        public (int totalFloorPlans, int availableFloorPlans, int totalLocationsOnFloorPlans) GetStatistics(List<FloorPlan> floorPlans)
        {
            if (floorPlans == null || floorPlans.Count == 0)
                return (0, 0, 0);

            var available = floorPlans.Count(f => f.IsAvailable);
            var totalLocations = floorPlans.Sum(f => f.LocationCount);

            return (floorPlans.Count, available, totalLocations);
        }

        /// <summary>
        /// Format floor display text
        /// </summary>
        public string FormatFloorDisplay(string floorLevel)
        {
            return floorLevel.ToUpper() switch
            {
                "G" => "Ground Floor",
                var x when x.StartsWith("B") => $"Basement {x.Substring(1)}",
                _ => $"Floor {floorLevel}"
            };
        }
    }
}
