using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MunicipalityApp.ViewModels
{
    public class ReportIssueViewModel
    {
        [Required(ErrorMessage = "Location is required.")]
        //[StringLength(200, ErrorMessage = "Location must be at most 200 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        //[StringLength(100, ErrorMessage = "Category must be at most 100 characters.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        //[StringLength(4000, ErrorMessage = "Description must be at most 4000 characters.")]
        public string Description { get; set; } = string.Empty;

        [DataType(DataType.Upload)]
        public IFormFile? Attachment { get; set; }
    }
}
