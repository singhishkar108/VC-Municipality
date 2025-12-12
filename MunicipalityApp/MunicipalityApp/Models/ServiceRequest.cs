using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MunicipalityApp.Models
{
    public class ServiceRequest
    {
        [Required]
        public Guid RequestID { get; set; } = Guid.NewGuid();

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ServiceType { get; set; } = string.Empty; // e.g., "Water & Sanitation"

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string StreetNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string StreetAddressLine2 { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Suburb { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string PostalCode { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        public DateTime DateRequested { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Requested"; // Default initial status

        [Range(1, 5)]
        public int PriorityLevel { get; set; } = 3; // Default: Medium

        [Required]
        public string AssignedDepartment { get; set; } = "Unassigned";

        public DateTime? CompletedDate { get; set; }

        public List<string> Attachments { get; set; } = new();

        // Citizen Contact Information (Manually Entered)
        [Required, StringLength(100)]
        public string CitizenName { get; set; } = string.Empty; // Now stores the manually entered name

        [Required, StringLength(100)]
        public string CitizenSurname { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string CitizenEmail { get; set; } = string.Empty;

        [Required, Phone]
        public string CitizenCellNumber { get; set; } = string.Empty;

        // NEW: Backend tracking variable for the logged-in user
        [StringLength(100)]
        public string TrackingUsername { get; set; } = string.Empty;
    }
}