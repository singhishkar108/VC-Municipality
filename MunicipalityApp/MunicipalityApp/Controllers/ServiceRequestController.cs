using Microsoft.AspNetCore.Mvc;
using MunicipalityApp.Models;
using MunicipalityApp.Services;
using MunicipalityApp.ViewModels;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;

namespace MunicipalityApp.Controllers
{
    public class ServiceRequestController : Controller
    {
        private readonly ServiceRequestService _serviceRequestService;

        // FIXED: Declare the BaseUploadDirectory as a private constant at the class level
        private const string BaseUploadDirectory = @"C:\Users\ishka\OneDrive\Documents\VS Code\MunicipalityApp\MunicipalityApp\wwwroot\uploads\servicerequest";

        private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", "mov", "mkv",
          ".pdf", ".doc", ".docx", ".odt", ".rtf", ".txt" };

        public ServiceRequestController(ServiceRequestService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService;
        }

        // --- AUTHENTICATION HELPERS ---

        // Checks if *any* user is logged in (User OR Admin)
        private bool IsUser()
        {
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username")) && (role == "User" || role == "Admin");
        }

        // Checks if the user is logged in AND their role is strictly "User"
        private bool IsOnlyUser()
        {
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username")) && role == "User";
        }

        // Checks if the user is logged in AND their role is strictly "Admin"
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Username")) && role == "Admin";
        }

        // --- END AUTHENTICATION HELPERS ---


        private bool IsAllowedMedia(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }

        private void SetViewBags()
        {
            ViewBag.ServiceTypes = new[]
            {
                "Water & Sanitation", "Electricity & Street Lighting", "Roads & Transport",
                "Parks & Recreation", "Waste Management", "Housing & Infrastructure",
                "Health & Safety", "Community Services", "Environmental Services", "Public Lighting"
            };
            ViewBag.Departments = new[]
            {
                "Water & Sanitation Department", "Electricity & Energy Department", "Roads & Transport Department",
                "Waste Management Department", "Parks & Recreation Department", "Housing & Infrastructure Department",
                "Health & Safety Department", "Community Services Department", "Environmental Affairs Department",
                "Public Lighting Department", "ICT & Customer Support Department", "Finance & Billing Department",
                "Emergency & Disaster Management Department", "Unassigned"
            };
            ViewBag.Statuses = new[]
            {
                "Requested", "Acknowledged", "Assigned", "In Progress", "On Hold", "Completed", "Cancelled"
            };
            ViewBag.Priorities = new[] { 5, 4, 3, 2, 1 };
        }

        [HttpGet]
        public IActionResult Create()
        {
            // SECURITY: Must be logged in ONLY as User (Requirement 1)
            if (!IsOnlyUser())
                return RedirectToAction("Login", "Auth");

            SetViewBags();
            return View();
        }

        // --------------------------------------------------------------------------------------------------
        // UPDATED: Create (Uses BaseUploadDirectory)
        // --------------------------------------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ServiceRequestViewModel model)
        {
            // SECURITY: Must be logged in ONLY as User (Requirement 1)
            if (!IsOnlyUser())
                return RedirectToAction("Login", "Auth");

            var trackingUsername = HttpContext.Session.GetString("Username") ?? "Anonymous";

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors in the form.";
                SetViewBags();
                return View(model);
            }

            var attachments = new List<string>();
            if (model.Attachments != null)
            {
                // Ensure the absolute directory exists
                Directory.CreateDirectory(BaseUploadDirectory);

                foreach (var file in model.Attachments)
                {
                    if (!IsAllowedMedia(file.FileName))
                    {
                        TempData["ErrorMessage"] = $"Invalid file type: {file.FileName}";
                        SetViewBags();
                        return View(model);
                    }

                    // Generate a unique filename to prevent overwrites
                    var fileExtension = Path.GetExtension(file.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

                    // Construct the full absolute file path using the requested directory
                    var filePath = Path.Combine(BaseUploadDirectory, uniqueFileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    file.CopyTo(stream);

                    // Store the relative web path for later access (Download and viewing)
                    attachments.Add("/uploads/servicerequest/" + uniqueFileName);
                }
            }

            var request = new ServiceRequest
            {
                Title = model.Title,
                ServiceType = model.ServiceType,
                Description = model.Description,
                StreetNumber = model.StreetNumber,
                StreetAddressLine2 = model.StreetAddressLine2,
                Suburb = model.Suburb,
                City = model.City,
                PostalCode = model.PostalCode,
                // Latitude = model.Latitude,
                // Longitude = model.Longitude,
                PriorityLevel = model.PriorityLevel,
                AssignedDepartment = model.AssignedDepartment,
                Attachments = attachments,

                CitizenName = model.CitizenName,
                CitizenSurname = model.CitizenSurname,
                CitizenEmail = model.CitizenEmail,
                CitizenCellNumber = model.CitizenCellNumber,

                TrackingUsername = trackingUsername
            };

            _serviceRequestService.AddRequest(request);
            TempData["SuccessMessage"] = "Service request submitted successfully!";
            return RedirectToAction("Dashboard");
        }


        [HttpGet]
        public IActionResult ListUserRequests()
        {
            // SECURITY: Must be logged in ONLY as User (Requirement 1)
            if (!IsOnlyUser())
                return RedirectToAction("Login", "Auth");

            SetViewBags();
            var username = HttpContext.Session.GetString("Username") ?? "";

            var requests = _serviceRequestService.GetAllRequests()
                                 .Where(r => r.TrackingUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                                 .ToList();
            return View("List", requests);
        }

        [HttpGet]
        public IActionResult Details(Guid id)
        {
            // SECURITY: No login needed (Public access for everyone) (Requirement 2)

            var request = _serviceRequestService.GetRequest(id);
            if (request == null)
                return NotFound();

            // Note: Since this is the public view, we don't need SetViewBags() 
            // unless the Details view uses them (e.g., for dropdowns, which it shouldn't).
            // It will render Views/ServiceRequest/Details.cshtml by default.
            return View(request);
        }

        [HttpGet]
        public IActionResult DetailsTwo(Guid id)
        {
            // SECURITY: No login needed (Public access for everyone) (Requirement 2)

            var request = _serviceRequestService.GetRequest(id);
            if (request == null)
                return NotFound();

            // Note: Since this is the public view, we don't need SetViewBags() 
            // unless the Details view uses them (e.g., for dropdowns, which it shouldn't).
            // It will render Views/ServiceRequest/Details.cshtml by default.
            return View(request);
        }


        [HttpGet]
        public IActionResult AdminIndex()
        {
            // SECURITY: Must be logged in as Admin
            if (!IsAdmin())
                return RedirectToAction("Login", "Auth");

            SetViewBags();
            var requests = _serviceRequestService.GetAllRequests().ToList();
            return View("AdminIndex", requests);
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            // SECURITY: No login needed (Public access for everyone) (Requirement 2)
            SetViewBags();
            ViewBag.PendingCount = _serviceRequestService.GetPendingRequestCount();
            var requests = _serviceRequestService.GetAllRequests().ToList();
            return View("Dashboard", requests);
        }

        [HttpGet]
        // Renamed from FilterAdminRequests to FilterDashboardRequests, 
        // as this handles the public dashboard data.
        public IActionResult FilterDashboardRequests(
            string searchTerm,
            string status,
            string priority,
            string assignedDepartment,
            string serviceType,
            string dateRequestedSort,
            DateTime? dateRequestedStart,
            DateTime? dateRequestedEnd,
            string completedDateSort,
            DateTime? completedDateStart,
            DateTime? completedDateEnd
        )
        {
            // SECURITY: No login needed (Public access for everyone) (Requirement 2)

            // 1. Get ALL requests (NO USERNAME FILTER)
            IEnumerable<ServiceRequest> requests = _serviceRequestService.GetAllRequests();

            // 2. Apply comprehensive search filter 
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                requests = requests.Where(r =>
                    // Title/Type/Status/Description/Location - Citizen Details removed for public view
                    (!string.IsNullOrEmpty(r.Title) && r.Title.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.ServiceType) && r.ServiceType.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Status) && r.Status.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Description) && r.Description.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.StreetNumber) && r.StreetNumber.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.StreetAddressLine2) && r.StreetAddressLine2.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Suburb) && r.Suburb.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.City) && r.City.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.PostalCode) && r.PostalCode.ToLower().Contains(searchTerm))
                );
            }

            // --- Apply Filters ---
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(priority) && !priority.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(priority, out int pLevel))
                {
                    requests = requests.Where(r => r.PriorityLevel == pLevel);
                }
            }
            if (!string.IsNullOrEmpty(assignedDepartment) && !assignedDepartment.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.AssignedDepartment, assignedDepartment, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(serviceType) && !serviceType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.ServiceType, serviceType, StringComparison.OrdinalIgnoreCase));
            }

            // --- Apply Date Range Filters ---
            if (dateRequestedStart.HasValue)
            {
                requests = requests.Where(r => r.DateRequested.Date >= dateRequestedStart.Value.Date);
            }
            if (dateRequestedEnd.HasValue)
            {
                requests = requests.Where(r => r.DateRequested.Date <= dateRequestedEnd.Value.Date);
            }
            if (completedDateStart.HasValue)
            {
                requests = requests.Where(r => r.CompletedDate.HasValue && r.CompletedDate.Value.Date >= completedDateStart.Value.Date);
            }
            if (completedDateEnd.HasValue)
            {
                requests = requests.Where(r => r.CompletedDate.HasValue && r.CompletedDate.Value.Date <= completedDateEnd.Value.Date);
            }

            // --- Apply Sorting Logic ---
            if (completedDateSort?.ToLower() == "asc")
            {
                requests = requests.OrderBy(r => r.CompletedDate);
            }
            else if (completedDateSort?.ToLower() == "desc")
            {
                requests = requests.OrderByDescending(r => r.CompletedDate);
            }
            else if (dateRequestedSort?.ToLower() == "asc")
            {
                requests = requests.OrderBy(r => r.DateRequested);
            }
            else if (dateRequestedSort?.ToLower() == "desc")
            {
                requests = requests.OrderByDescending(r => r.DateRequested);
            }
            // Default sort (e.g., by DateRequested Descending if no sort is specified)
            else
            {
                requests = requests.OrderByDescending(r => r.DateRequested);
            }

            // --- Format result for AJAX (Public view) ---
            var result = requests.Select(r => new
            {
                r.RequestID,
                r.Title,
                r.Description,
                r.ServiceType,
                Location = $"{r.StreetNumber}, {r.Suburb}", // Simplified location for public view
                DateRequested = r.DateRequested.ToString("yyyy-MM-dd"),
                r.Status,
                r.PriorityLevel,
                r.AssignedDepartment,
                // Citizen Details REMOVED for public view

                AttachmentCount = r.Attachments?.Count ?? 0,

                CompletedDate = r.Status == "Cancelled"
                    ? "Service Request Cancelled"
                    : (r.CompletedDate.HasValue
                        ? r.CompletedDate.Value.ToString("yyyy-MM-dd")
                        : "N/A")
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public IActionResult FilterAdminRequests( // Admin specific filter to include sensitive search terms
            string searchTerm,
            string status,
            string priority,
            string assignedDepartment,
            string serviceType,
            string dateRequestedSort,
            DateTime? dateRequestedStart,
            DateTime? dateRequestedEnd,
            string completedDateSort,
            DateTime? completedDateStart,
            DateTime? completedDateEnd
        )
        {
            // SECURITY: Must be logged in as Admin
            if (!IsAdmin())
                return Unauthorized();

            // 1. Get ALL requests 
            IEnumerable<ServiceRequest> requests = _serviceRequestService.GetAllRequests();

            // 2. Apply comprehensive search filter (Admin search includes sensitive fields)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                requests = requests.Where(r =>
                    // Title/Type/Status/Description/Location
                    (!string.IsNullOrEmpty(r.Title) && r.Title.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.ServiceType) && r.ServiceType.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Status) && r.Status.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Description) && r.Description.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.StreetNumber) && r.StreetNumber.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.StreetAddressLine2) && r.StreetAddressLine2.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Suburb) && r.Suburb.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.City) && r.City.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.PostalCode) && r.PostalCode.ToLower().Contains(searchTerm)) ||
                    // User Details (NEW)
                    (!string.IsNullOrEmpty(r.CitizenName) && r.CitizenName.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.CitizenSurname) && r.CitizenSurname.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.CitizenEmail) && r.CitizenEmail.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.CitizenCellNumber) && r.CitizenCellNumber.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.TrackingUsername) && r.TrackingUsername.ToLower().Contains(searchTerm))
                );
            }

            // --- Apply Filters --- (same as before)
            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(priority) && !priority.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(priority, out int pLevel))
                {
                    requests = requests.Where(r => r.PriorityLevel == pLevel);
                }
            }
            if (!string.IsNullOrEmpty(assignedDepartment) && !assignedDepartment.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.AssignedDepartment, assignedDepartment, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(serviceType) && !serviceType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.ServiceType, serviceType, StringComparison.OrdinalIgnoreCase));
            }

            // --- Apply Date Range Filters --- (same as before)
            if (dateRequestedStart.HasValue)
            {
                requests = requests.Where(r => r.DateRequested.Date >= dateRequestedStart.Value.Date);
            }
            if (dateRequestedEnd.HasValue)
            {
                requests = requests.Where(r => r.DateRequested.Date <= dateRequestedEnd.Value.Date);
            }
            if (completedDateStart.HasValue)
            {
                requests = requests.Where(r => r.CompletedDate.HasValue && r.CompletedDate.Value.Date >= completedDateStart.Value.Date);
            }
            if (completedDateEnd.HasValue)
            {
                requests = requests.Where(r => r.CompletedDate.HasValue && r.CompletedDate.Value.Date <= completedDateEnd.Value.Date);
            }

            // --- Apply Sorting Logic --- (same as before)
            if (completedDateSort?.ToLower() == "asc")
            {
                requests = requests.OrderBy(r => r.CompletedDate);
            }
            else if (completedDateSort?.ToLower() == "desc")
            {
                requests = requests.OrderByDescending(r => r.CompletedDate);
            }
            else if (dateRequestedSort?.ToLower() == "asc")
            {
                requests = requests.OrderBy(r => r.DateRequested);
            }
            else if (dateRequestedSort?.ToLower() == "desc")
            {
                requests = requests.OrderByDescending(r => r.DateRequested);
            }
            // Default sort (e.g., by DateRequested Descending if no sort is specified)
            else
            {
                requests = requests.OrderByDescending(r => r.DateRequested);
            }

            // --- Format result for AJAX (Admin view) ---
            var result = requests.Select(r => new
            {
                r.RequestID,
                r.Title,
                r.Description,
                r.ServiceType,
                Location = $"{r.StreetNumber}, {r.StreetAddressLine2}, {r.Suburb}, {r.City}, {r.PostalCode}, {r.Latitude}, {r.Longitude}",
                DateRequested = r.DateRequested.ToString("yyyy-MM-dd"),
                r.Status,
                r.PriorityLevel,
                r.AssignedDepartment,
                CitizenDetails = $"{r.CitizenName}, {r.CitizenSurname}, {r.CitizenEmail}, {r.CitizenCellNumber}, {r.TrackingUsername}",

                AttachmentCount = r.Attachments?.Count ?? 0,

                CompletedDate = r.Status == "Cancelled"
                    ? "Service Request Cancelled"
                    : (r.CompletedDate.HasValue
                        ? r.CompletedDate.Value.ToString("yyyy-MM-dd")
                        : "N/A")
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public IActionResult AdminDetails(Guid id)
        {
            // SECURITY: Must be logged in as Admin
            if (!IsAdmin())
                return RedirectToAction("Login", "Auth");

            SetViewBags();

            var request = _serviceRequestService.GetRequest(id);
            if (request == null)
                return NotFound();

            // GRAPH/BFS INTEGRATION: Get all possible downstream statuses
            ViewBag.StatusProgression = _serviceRequestService.GetStatusProgression(request.Status);
            // GRAPH/NEIGHBORS INTEGRATION: Get only the immediate, valid transitions
            ViewBag.NextStatuses = _serviceRequestService.GetValidNextStatuses(request.Status);

            return View("AdminDetails", request);
        }

        // ================================
        // POST: Update Property via AJAX
        // ================================
        [HttpPost]
        public IActionResult UpdateServiceRequestProperty(Guid id, string propertyName, string newValue)
        {
            // SECURITY: Must be logged in as Admin
            if (!IsAdmin())
                return Unauthorized();

            var request = _serviceRequestService.GetRequest(id);
            if (request == null)
                return NotFound(new { success = false, message = "Request not found." });

            DateTime? newCompletedDate = request.CompletedDate;

            // Use reflection or a switch/case structure to update the property
            switch (propertyName)
            {
                case "ServiceType":
                    request.ServiceType = newValue;
                    break;
                case "AssignedDepartment":
                    request.AssignedDepartment = newValue;
                    break;
                case "Status":
                    request.Status = newValue;

                    if (newValue == "Completed")
                    {
                        // Set to exact current time
                        newCompletedDate = DateTime.Now;
                    }
                    else if (newValue == "Cancelled")
                    {
                        // Set CompletedDate to null, view logic handles the custom text
                        newCompletedDate = null;
                    }
                    else
                    {
                        // For any other status change, clear CompletedDate
                        newCompletedDate = null;
                    }
                    request.CompletedDate = newCompletedDate;
                    break;
                case "PriorityLevel":
                    if (int.TryParse(newValue, out int pLevel))
                    {
                        // The Min-Heap in ServiceRequestService handles the re-heapify on update
                        request.PriorityLevel = pLevel;
                    }
                    break;
                default:
                    return BadRequest(new { success = false, message = $"Property {propertyName} is not editable." });
            }

            bool success = _serviceRequestService.UpdateRequest(request);

            if (success)
            {
                string completedDateDisplay;
                if (request.Status == "Completed" && request.CompletedDate.HasValue)
                {
                    completedDateDisplay = request.CompletedDate.Value.ToString("yyyy-MM-dd");
                }
                else if (request.Status == "Cancelled")
                {
                    completedDateDisplay = "Service Request Cancelled";
                }
                else
                {
                    completedDateDisplay = "N/A";
                }

                return Json(new
                {
                    success = true,
                    message = $"{propertyName} updated.",
                    completedDate = completedDateDisplay
                });
            }

            return BadRequest(new { success = false, message = $"Failed to update {propertyName}." });
        }

        [HttpGet]
        public IActionResult FilterUserRequests(
            string searchTerm,
            string status,
            string priority,
            string assignedDepartment,
            string serviceType,
            string dateRequestedSort,
            DateTime? dateRequestedStart,
            DateTime? dateRequestedEnd,
            string completedDateSort,
            DateTime? completedDateStart,
            DateTime? completedDateEnd
        )
        {
            // SECURITY: Must be logged in as User (Covers both regular user and admin access)
            if (!IsUser())
                return Unauthorized();

            var username = HttpContext.Session.GetString("Username") ?? "";

            IEnumerable<ServiceRequest> requests = _serviceRequestService.GetAllRequests()
                                 .Where(r => string.Equals(r.TrackingUsername, username, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                requests = requests.Where(r =>
                    (!string.IsNullOrEmpty(r.Title) && r.Title.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.ServiceType) && r.ServiceType.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Status) && r.Status.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Description) && r.Description.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.StreetNumber) && r.StreetNumber.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.StreetAddressLine2) && r.StreetAddressLine2.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.Suburb) && r.Suburb.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(r.City) && r.City.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(priority) && !priority.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(priority, out int pLevel))
                {
                    requests = requests.Where(r => r.PriorityLevel == pLevel);
                }
            }

            if (!string.IsNullOrEmpty(assignedDepartment) && !assignedDepartment.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.AssignedDepartment, assignedDepartment, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(serviceType) && !serviceType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                requests = requests.Where(r => string.Equals(r.ServiceType, serviceType, StringComparison.OrdinalIgnoreCase));
            }

            if (dateRequestedStart.HasValue)
            {
                requests = requests.Where(r => r.DateRequested.Date >= dateRequestedStart.Value.Date);
            }
            if (dateRequestedEnd.HasValue)
            {
                requests = requests.Where(r => r.DateRequested.Date <= dateRequestedEnd.Value.Date);
            }

            if (completedDateStart.HasValue)
            {
                requests = requests.Where(r => r.CompletedDate.HasValue && r.CompletedDate.Value.Date >= completedDateStart.Value.Date);
            }
            if (completedDateEnd.HasValue)
            {
                requests = requests.Where(r => r.CompletedDate.HasValue && r.CompletedDate.Value.Date <= completedDateEnd.Value.Date);
            }

            if (completedDateSort?.ToLower() == "asc")
            {
                requests = requests.OrderBy(r => r.CompletedDate);
            }
            else if (completedDateSort?.ToLower() == "desc")
            {
                requests = requests.OrderByDescending(r => r.CompletedDate);
            }
            else if (dateRequestedSort?.ToLower() == "asc")
            {
                requests = requests.OrderBy(r => r.DateRequested);
            }
            else if (dateRequestedSort?.ToLower() == "desc")
            {
                requests = requests.OrderByDescending(r => r.DateRequested);
            }

            var result = requests.Select(r => new
            {
                r.RequestID,
                r.Title,
                r.Description,
                r.ServiceType,
                Location = $"{r.StreetNumber}, {r.StreetAddressLine2}, {r.Suburb}, {r.City}, {r.PostalCode}, {r.Latitude}, {r.Longitude}",
                DateRequested = r.DateRequested.ToString("yyyy-MM-dd"),
                r.Status,
                r.PriorityLevel,
                r.AssignedDepartment,
                CompletedDate = r.Status == "Cancelled"
                    ? "Service Request Cancelled"
                    : (r.CompletedDate.HasValue
                        ? r.CompletedDate.Value.ToString("yyyy-MM-dd")
                        : "N/A")
            }).ToList();

            return Json(result);
        }

        // --------------------------------------------------------------------------------------------------
        // UPDATED: DownloadAttachments (Uses BaseUploadDirectory)
        // --------------------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult DownloadAttachments(Guid requestId)
        {
            var request = _serviceRequestService.GetRequest(requestId);
            if (request == null || request.Attachments.Count == 0)
                return NotFound();

            var zipFileName = $"Attachments_{requestId}.zip";
            var zipPath = Path.Combine(Path.GetTempPath(), zipFileName);

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var fileUrl in request.Attachments)
                {
                    // The fileUrl is expected to be "/uploads/servicerequest/filename.ext".
                    var fileName = Path.GetFileName(fileUrl);

                    // Construct the absolute path for retrieval using the class-level constant
                    var filePath = Path.Combine(BaseUploadDirectory, fileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        // Create the zip entry using only the filename to avoid folder structure in the zip
                        zip.CreateEntryFromFile(filePath, fileName);
                    }
                }
            }

            var bytes = System.IO.File.ReadAllBytes(zipPath);
            System.IO.File.Delete(zipPath);

            return File(bytes, "application/zip", zipFileName);
        }

        [HttpGet]
        public IActionResult NextHighPriority()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Auth");

            var request = _serviceRequestService.GetNextHighPriority();

            if (request == null)
            {
                TempData["Message"] = "No pending requests found in the priority queue.";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Dashboard", "ServiceRequest");
            }

            return RedirectToAction("Dashboard", "ServiceRequest", new { id = request.RequestID });
        }
    }
}