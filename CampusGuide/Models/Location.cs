using System.Collections.Generic;

namespace CampusGuide.Models
{
    public class Location
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Coordinates Coordinates { get; set; } = new Coordinates();
        public string Floor { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public string QrCodeId { get; set; } = string.Empty;
    }
}
