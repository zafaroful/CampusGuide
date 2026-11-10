using CampusGuide.Models;
using System.Diagnostics;

namespace CampusGuide.Services
{
    /// <summary>
    /// Service for clustering nearby map pins into groups.
    /// Uses distance-based clustering algorithm.
    /// </summary>
    public class ClusteringService
    {
        /// <summary>
        /// Default clustering radius in kilometers
        /// Pins within this distance are grouped into one cluster
        /// </summary>
        private const double DefaultClusterRadiusKm = 0.5; // 500 meters

        /// <summary>
        /// Cache for clustering results to avoid redundant calculations
        /// </summary>
        private List<ClusterPin>? _lastClusterResult;
        private List<MapPin>? _lastPinList;
        private double _lastClusterRadius;
        private int _lastPinListHash;

        /// <summary>
        /// Cluster nearby pins based on proximity
        /// </summary>
        public List<ClusterPin> ClusterPins(List<MapPin> pins, double clusterRadiusKm = DefaultClusterRadiusKm)
        {
            if (pins == null || pins.Count == 0)
            {
                Debug.WriteLine("ClusteringService: No pins to cluster");
                return new List<ClusterPin>();
            }

            // Check cache for same input
            var currentHash = CalculateListHash(pins);
            if (_lastClusterResult != null && 
                _lastPinListHash == currentHash && 
                Math.Abs(_lastClusterRadius - clusterRadiusKm) < 0.0001)
            {
                Debug.WriteLine("ClusteringService: Using cached cluster result");
                return _lastClusterResult;
            }

            var clusters = new List<ClusterPin>();
            var processedIndices = new HashSet<int>();

            Debug.WriteLine($"ClusteringService: Clustering {pins.Count} pins with radius {clusterRadiusKm} km");

            for (int i = 0; i < pins.Count; i++)
            {
                // Skip already clustered pins
                if (processedIndices.Contains(i))
                    continue;

                var currentPin = pins[i];
                var nearbyPins = new List<MapPin> { currentPin };
                processedIndices.Add(i);

                // Find all pins within clustering radius
                for (int j = i + 1; j < pins.Count; j++)
                {
                    if (processedIndices.Contains(j))
                        continue;

                    var otherPin = pins[j];
                    var distance = CalculateDistance(
                        currentPin.Coordinates.Latitude,
                        currentPin.Coordinates.Longitude,
                        otherPin.Coordinates.Latitude,
                        otherPin.Coordinates.Longitude
                    );

                    if (distance <= clusterRadiusKm)
                    {
                        nearbyPins.Add(otherPin);
                        processedIndices.Add(j);
                    }
                }

                // Create cluster
                var cluster = new ClusterPin
                {
                    Pins = nearbyPins
                };
                cluster.CalculateClusterProperties();
                clusters.Add(cluster);

                Debug.WriteLine($"ClusteringService: Created cluster with {cluster.PinCount} pins");
            }

            // Cache result
            _lastClusterResult = clusters;
            _lastPinList = pins;
            _lastClusterRadius = clusterRadiusKm;
            _lastPinListHash = currentHash;

            Debug.WriteLine($"ClusteringService: Generated {clusters.Count} clusters from {pins.Count} pins");
            return clusters;
        }

        /// <summary>
        /// Get clusters for current map zoom level
        /// </summary>
        public List<ClusterPin> GetClustersForZoomLevel(List<ClusterPin> allClusters, double zoomLevel)
        {
            // At high zoom (far out), return clusters
            // At low zoom (close in), return individual pins from clusters

            if (zoomLevel < 0.05) // Very zoomed in
            {
                // Return individual pins
                var individualPins = new List<ClusterPin>();
                foreach (var cluster in allClusters)
                {
                    if (cluster.IsCluster && cluster.IsExpanded)
                    {
                        // Create individual cluster pins
                        foreach (var pin in cluster.Pins)
                        {
                            var singleCluster = new ClusterPin
                            {
                                Pins = new List<MapPin> { pin }
                            };
                            singleCluster.CalculateClusterProperties();
                            individualPins.Add(singleCluster);
                        }
                    }
                    else
                    {
                        individualPins.Add(cluster);
                    }
                }
                return individualPins;
            }

            // Return all clusters as-is
            return allClusters;
        }

        /// <summary>
        /// Calculate distance between two coordinates (Haversine formula)
        /// Returns distance in kilometers
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
        /// Expand cluster (show individual pins)
        /// </summary>
        public void ExpandCluster(ClusterPin cluster)
        {
            if (cluster == null || !cluster.IsCluster)
                return;

            cluster.IsExpanded = true;
            Debug.WriteLine($"ClusteringService: Expanded cluster with {cluster.PinCount} pins");
        }

        /// <summary>
        /// Collapse cluster (hide individual pins)
        /// </summary>
        public void CollapseCluster(ClusterPin cluster)
        {
            if (cluster == null)
                return;

            cluster.IsExpanded = false;
            Debug.WriteLine($"ClusteringService: Collapsed cluster");
        }

        /// <summary>
        /// Check if two coordinates are close enough to be in same cluster
        /// </summary>
        public bool ArePointsNearby(Coordinates point1, Coordinates point2, double radiusKm = DefaultClusterRadiusKm)
        {
            var distance = CalculateDistance(
                point1.Latitude, point1.Longitude,
                point2.Latitude, point2.Longitude
            );
            return distance <= radiusKm;
        }

        /// <summary>
        /// Get clustering statistics
        /// </summary>
        public (int totalPins, int totalClusters, int clusteredPins, double avgClusterSize) GetStatistics(List<ClusterPin> clusters)
        {
            var totalPins = clusters.Sum(c => c.PinCount);
            var clusterCount = clusters.Count;
            var clusteredPinsCount = clusters.Where(c => c.IsCluster).Sum(c => c.PinCount);
            var avgSize = clusterCount > 0 ? totalPins / (double)clusterCount : 0;

            return (totalPins, clusterCount, clusteredPinsCount, avgSize);
        }

        /// <summary>
        /// Calculate hash of pin list for caching
        /// </summary>
        private int CalculateListHash(List<MapPin> pins)
        {
            unchecked
            {
                int hash = 17;
                foreach (var pin in pins)
                {
                    hash = hash * 31 + pin.LocationId.GetHashCode();
                    hash = hash * 31 + pin.Coordinates.Latitude.GetHashCode();
                    hash = hash * 31 + pin.Coordinates.Longitude.GetHashCode();
                }
                return hash;
            }
        }

        /// <summary>
        /// Clear clustering cache
        /// </summary>
        public void ClearCache()
        {
            _lastClusterResult = null;
            _lastPinList = null;
            Debug.WriteLine("ClusteringService: Cache cleared");
        }
    }
}
