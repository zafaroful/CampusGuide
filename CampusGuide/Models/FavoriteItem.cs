using System.Linq;

namespace CampusGuide.Models
{
    public class FavoriteItem
    {
        public string Id { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;

        public string MetaText => string.Join(" · ", new[]
        {
            Type,
            Building,
            string.IsNullOrWhiteSpace(Floor) ? string.Empty : $"Floor {Floor}"
        }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}
