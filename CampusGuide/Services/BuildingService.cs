using CampusGuide.Models;
using System.Diagnostics;
using LocationModel = CampusGuide.Models.Location;

namespace CampusGuide.Services
{
    /// <summary>
    /// Service for managing buildings and building-based filtering.
    /// Extracts buildings from locations and provides filtering utilities.
    /// </summary>
    public class BuildingService
    {
        /// <summary>
        /// Extract unique buildings from a list of locations
        /// </summary>
        public List<Building> ExtractBuildingsFromLocations(List<LocationModel> locations)
        {
            if (locations == null || locations.Count == 0)
            {
                Debug.WriteLine("BuildingService: No locations to extract buildings from");
                return new List<Building>();
            }

            Debug.WriteLine($"BuildingService: Extracting buildings from {locations.Count} locations");

            // Group locations by building name
            var buildingGroups = locations
                .GroupBy(l => l.Building ?? "Unknown Building")
                .ToList();

            var buildings = new List<Building>();

            foreach (var group in buildingGroups)
            {
                var building = Building.FromLocations(group.Key, group.ToList());
                buildings.Add(building);
                Debug.WriteLine($"BuildingService: Created building '{building.Name}' with {building.LocationCount} locations");
            }

            // Sort by location count (most locations first)
            buildings = buildings
                .OrderByDescending(b => b.LocationCount)
                .ToList();

            Debug.WriteLine($"BuildingService: Extracted {buildings.Count} buildings");
            return buildings;
        }

        /// <summary>
        /// Filter locations by building
        /// </summary>
        public List<LocationModel> GetLocationsInBuilding(List<LocationModel> locations, string buildingName)
        {
            if (string.IsNullOrEmpty(buildingName))
            {
                Debug.WriteLine("BuildingService: No building filter specified, returning all locations");
                return locations ?? new List<LocationModel>();
            }

            var filtered = locations?
                .Where(l => (l.Building ?? "Unknown Building").Equals(buildingName, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<LocationModel>();

            Debug.WriteLine($"BuildingService: Filtered {filtered.Count} locations in building '{buildingName}'");
            return filtered;
        }

        /// <summary>
        /// Filter map pins by building
        /// </summary>
        public List<MapPin> GetMapPinsInBuilding(List<MapPin> pins, string buildingName)
        {
            if (string.IsNullOrEmpty(buildingName))
            {
                Debug.WriteLine("BuildingService: No building filter specified, returning all pins");
                return pins ?? new List<MapPin>();
            }

            var filtered = pins?
                .Where(p => p.Building.Equals(buildingName, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<MapPin>();

            Debug.WriteLine($"BuildingService: Filtered {filtered.Count} pins in building '{buildingName}'");
            return filtered;
        }

        /// <summary>
        /// Get all locations in multiple buildings
        /// </summary>
        public List<LocationModel> GetLocationsInBuildings(List<LocationModel> locations, List<string> buildingNames)
        {
            if (buildingNames == null || buildingNames.Count == 0)
                return locations ?? new List<LocationModel>();

            var filtered = locations?
                .Where(l => buildingNames.Contains(l.Building ?? "Unknown Building", StringComparer.OrdinalIgnoreCase))
                .ToList() ?? new List<LocationModel>();

            Debug.WriteLine($"BuildingService: Filtered {filtered.Count} locations in {buildingNames.Count} buildings");
            return filtered;
        }

        /// <summary>
        /// Get all unique floor levels in a building
        /// </summary>
        public List<string> GetFloorsInBuilding(List<LocationModel> locations, string buildingName)
        {
            if (string.IsNullOrEmpty(buildingName))
                return new List<string>();

            var floors = locations?
                .Where(l => (l.Building ?? "Unknown Building").Equals(buildingName, StringComparison.OrdinalIgnoreCase))
                .Select(l => l.Floor)
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct()
                .ToList() ?? new List<string>();

            Debug.WriteLine($"BuildingService: Found {floors.Count} floors in building '{buildingName}'");
            return floors;
        }

        /// <summary>
        /// Get locations on a specific floor in a building
        /// </summary>
        public List<LocationModel> GetLocationsOnFloor(List<LocationModel> locations, string buildingName, string floor)
        {
            if (string.IsNullOrEmpty(buildingName) || string.IsNullOrEmpty(floor))
                return new List<LocationModel>();

            var filtered = locations?
                .Where(l => (l.Building ?? "Unknown Building").Equals(buildingName, StringComparison.OrdinalIgnoreCase) &&
                           l.Floor.Equals(floor, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<LocationModel>();

            Debug.WriteLine($"BuildingService: Found {filtered.Count} locations on floor '{floor}' in building '{buildingName}'");
            return filtered;
        }

        /// <summary>
        /// Search within a building
        /// </summary>
        public List<LocationModel> SearchInBuilding(List<LocationModel> locations, string buildingName, string searchText)
        {
            if (string.IsNullOrEmpty(buildingName))
                return new List<LocationModel>();

            var searchLower = searchText?.ToLower() ?? string.Empty;

            var filtered = locations?
                .Where(l => (l.Building ?? "Unknown Building").Equals(buildingName, StringComparison.OrdinalIgnoreCase) &&
                           (l.Name.ToLower().Contains(searchLower) ||
                            l.Description.ToLower().Contains(searchLower) ||
                            l.Type.ToLower().Contains(searchLower) ||
                            l.Floor.ToLower().Contains(searchLower)))
                .ToList() ?? new List<LocationModel>();

            Debug.WriteLine($"BuildingService: Found {filtered.Count} locations matching '{searchText}' in building '{buildingName}'");
            return filtered;
        }

        /// <summary>
        /// Get floor range as string (e.g., "G-3" or "B1-G")
        /// </summary>
        public string GetFloorRange(Building building)
        {
            if (building?.Floors == null || building.Floors.Count == 0)
                return "";

            if (building.Floors.Count == 1)
                return building.Floors[0];

            return $"{building.Floors.First()}-{building.Floors.Last()}";
        }

        /// <summary>
        /// Get statistics about buildings
        /// </summary>
        public (int totalBuildings, int totalLocations, double avgLocationsPerBuilding) GetStatistics(List<Building> buildings)
        {
            if (buildings == null || buildings.Count == 0)
                return (0, 0, 0);

            var totalLocations = buildings.Sum(b => b.LocationCount);
            var avgPerBuilding = totalLocations / (double)buildings.Count;

            return (buildings.Count, totalLocations, avgPerBuilding);
        }

        /// <summary>
        /// Format floor display text
        /// </summary>
        public string FormatFloorDisplay(string floor)
        {
            if (string.IsNullOrEmpty(floor))
                return "Unknown Floor";

            return floor.ToUpper() switch
            {
                "G" => "Ground Floor",
                var x when x.StartsWith("B") => $"Basement {x.Substring(1)}",
                _ => $"Floor {floor}"
            };
        }
    }
}
