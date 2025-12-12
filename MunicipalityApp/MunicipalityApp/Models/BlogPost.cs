using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MunicipalityApp.Models
{
    public class BlogPost
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string? MediaPath { get; set; }

        public List<string> Hashtags { get; set; } = new();

        public int Likes { get; set; } = 0;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow.AddHours(2);

        public string ShareUrl { get; set; } = string.Empty;

        // To prevent duplicate likes by the same user
        public HashSet<string> LikedBy { get; set; } = new();
    }
}
