using System;

namespace CampusGuide.Models
{
    public class EventItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string LocationId { get; set; } = string.Empty;
        public Coordinates Coordinates { get; set; } = new Coordinates();
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
    }
}
