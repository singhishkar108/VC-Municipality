using Microsoft.AspNetCore.Mvc;
using MunicipalityApp.Models;
using MunicipalityApp.Services;
using MunicipalityApp.ViewModels;
using System.Linq;

namespace MunicipalityApp.Controllers
{
    public class IssueController : Controller
    {
        private readonly IssueService _issueService;

        // Updated AllowedExtensions to include document file types
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov", ".mkv", ".pdf", ".doc", ".docx", ".odt", ".rtf", ".txt" };

        public IssueController(IssueService issueService)
        {
            _issueService = issueService;
        }

        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        private bool IsAdmin() => IsLoggedIn() && HttpContext.Session.GetString("Role") == "Admin";
        private bool IsUser() => IsLoggedIn() && HttpContext.Session.GetString("Role") == "User";

        private bool IsAllowedMedia(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }

        // ========== USER SIDE ==========
        [HttpGet]
        public IActionResult Report()
        {
            if (!IsUser())
                return RedirectToAction("Login", "Auth");

            ViewBag.Categories = new List<string>
            {
                "Roads",
                "Electricity",
                "Water",
                "Sanitation",
                "Waste Management",
                "Healthcare",
                "Education",
                "Other"
            };

            ViewBag.Leaderboard = _issueService.GetLeaderboard();
            return View();
        }

        [HttpPost]
        public IActionResult Report(ReportIssueViewModel model)
        {
            if (!IsUser())
                return RedirectToAction("Login", "Auth");

            // No need for ModelState.IsValid check as we are doing custom file validation
            var username = HttpContext.Session.GetString("Username") ?? "Anonymous";
            var attachments = new List<string>();

            if (model.Attachment != null)
            {
                if (!IsAllowedMedia(model.Attachment))
                {
                    // Return a JSON response for toastr
                    return Json(new { success = false, message = "Invalid file format. Please attach a valid image, video, or document file." });
                }

                var fileName = Path.GetFileName(model.Attachment.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? "");

                using var stream = new FileStream(filePath, FileMode.Create);
                model.Attachment.CopyTo(stream);
                attachments.Add("/uploads/" + fileName);
            }

            var issue = new Issue
            {
                Username = username,
                Location = model.Location,
                Category = model.Category,
                Description = model.Description,
                Attachments = attachments
            };

            _issueService.AddIssue(issue);

            // Return a JSON response for toastr
            return Json(new { success = true, message = "Report submitted successfully! Thank you for your contribution." });
        }

        [HttpGet]
        public IActionResult List()
        {
            if (!IsUser())
                return RedirectToAction("Login", "Auth");

            var username = HttpContext.Session.GetString("Username") ?? "";
            var issues = _issueService.GetAllIssues().Where(i => i.Username == username).ToList();

            return View(issues);
        }

        // ========== ADMIN SIDE ==========
        [HttpGet]
        public IActionResult AllList(string? fromDate, string? toDate, string? searchQuery, string? usernameFilter, string? categoryFilter, string? progressFilter)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Auth");

            DateOnly? from = null, to = null;
            if (!string.IsNullOrWhiteSpace(fromDate) && DateOnly.TryParse(fromDate, out var f)) from = f;
            if (!string.IsNullOrWhiteSpace(toDate) && DateOnly.TryParse(toDate, out var t)) to = t;

            var issues = _issueService.GetAllIssues();

            // Filter by date range
            if (from.HasValue)
            {
                issues = issues.Where(i => DateOnly.FromDateTime(i.Timestamp.Date) >= from.Value).ToList();
            }
            if (to.HasValue)
            {
                issues = issues.Where(i => DateOnly.FromDateTime(i.Timestamp.Date) <= to.Value).ToList();
            }

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                issues = issues.Where(i =>
                    (i.Username?.ToLower().Contains(lowerQuery) ?? false) ||
                    (i.Location?.ToLower().Contains(lowerQuery) ?? false) ||
                    (i.Category?.ToLower().Contains(lowerQuery) ?? false) ||
                    (i.Description?.ToLower().Contains(lowerQuery) ?? false)
                ).ToList();
            }

            // Filter by selected user
            if (!string.IsNullOrWhiteSpace(usernameFilter))
            {
                issues = issues.Where(i => i.Username == usernameFilter).ToList();
            }

            // Filter by selected category
            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                issues = issues.Where(i => i.Category == categoryFilter).ToList();
            }

            // Filter by selected progress status
            if (!string.IsNullOrWhiteSpace(progressFilter))
            {
                issues = issues.Where(i => i.Progress == progressFilter).ToList();
            }

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.UsernameFilter = usernameFilter;
            ViewBag.CategoryFilter = categoryFilter;
            ViewBag.ProgressFilter = progressFilter;

            ViewBag.AllUsers = _issueService.GetAllIssues().Select(i => i.Username).Distinct().ToList();
            ViewBag.AllCategories = _issueService.GetAllIssues().Select(i => i.Category).Distinct().ToList();
            ViewBag.AllProgress = _issueService.GetAllIssues().Select(i => i.Progress).Distinct().ToList();

            return View(issues);
        }

        [HttpPost]
        public IActionResult UpdateProgressAjax(Guid issueId, string newProgress)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
                return Unauthorized(); // HTTP 401

            if (issueId == Guid.Empty || string.IsNullOrEmpty(newProgress))
                return BadRequest(); // HTTP 400

            bool updated = _issueService.UpdateIssueProgress(issueId, newProgress);

            if (updated)
            {
                return Json(new { success = true, message = "Progress updated successfully!" });
            }

            return Json(new { success = false, message = "Error updating progress!" });
        }
    }
}