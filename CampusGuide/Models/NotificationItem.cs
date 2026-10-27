using System;

namespace CampusGuide.Models
{
    public class NotificationItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RelatedEventId { get; set; } = string.Empty;
        public string RelatedLocationId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
