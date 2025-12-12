using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MunicipalityApp.ViewModels
{
    public class ServiceRequestViewModel
    {

        private const string DecimalRegex = @"^-?\d+(\.\d+)?$";

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ServiceType { get; set; } = string.Empty;

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

        // [RegularExpression(DecimalRegex, ErrorMessage = "Latitude must be a number using a dot (.) as the decimal separator (e.g., 34.00000).")]
        // public double? Latitude { get; set; }

        // [RegularExpression(DecimalRegex, ErrorMessage = "Latitude must be a number using a dot (.) as the decimal separator (e.g., 34.00000).")]
        // public double? Longitude { get; set; }

        // Change 3: Set default PriorityLevel to 1 in the ViewModel
        [Range(1, 5)]
        public int PriorityLevel { get; set; } = 1;

        [Required]
        public string AssignedDepartment { get; set; } = "Unassigned";

        public List<IFormFile>? Attachments { get; set; }

        // --- Change 1 & 2: Citizen/User Information ---
        [Required, StringLength(100)]
        public string CitizenName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string CitizenSurname { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string CitizenEmail { get; set; } = string.Empty;

        [Required, Phone]
        public string CitizenCellNumber { get; set; } = string.Empty;

        // This field will hold the actual logged-in user's username for tracking (Model/user.cs equivalent)
        public string TrackingUsername { get; set; } = string.Empty;
    }
}