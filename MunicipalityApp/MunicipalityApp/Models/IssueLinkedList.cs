using System;
using System.Collections.Generic;

namespace MunicipalityApp.Models
{
    public class Issue
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Attachments { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Progress { get; set; } = "Submitted";
    }
}
