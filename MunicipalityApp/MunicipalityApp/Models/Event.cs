using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MunicipalityApp.Models
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // NEW: Property to store the type (Event or Announcement)
        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? MediaPath { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public List<string> Hashtags { get; set; } = new List<string>();

        public string ShareUrl { get; set; } = string.Empty;
    }
}