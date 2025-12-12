using Microsoft.AspNetCore.Mvc;
using MunicipalityApp.Models;
using MunicipalityApp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace MunicipalityApp.Controllers
{
    //Handles all user and admin-side actions for Events and Announcements, 
    //including creation, listing, filtering, and media management.
    public class EventController : Controller
    {
        private readonly EventService _eventService;
        private readonly RecommendationService _recommendationService;
        private readonly IWebHostEnvironment _env;

        private static readonly List<string> DefaultCategories = new()
        {
            "Service Delivery Alert",
            "Road Closures & Traffic",
            "Public Participation & Meetings",
            "Job & Tenders",
            "Health & Safety Warning",
            "Council & Policy Notice",
            "Arts & Culture",
            "Sports & Recreation",
            "Markets & Festivals",
            "Family & Kids",
            "Learning & Skill Building",
            "Environmental & Green Initiatives"
        };

        public static readonly List<string> DefaultLocations = new()
        {
            "Berea", "Bluff", "Greyville", "Point Waterfront", "Chatsworth",
            "uMhlanga", "Durban North", "Tongaat", "Cornubia", "Ntuzuma",
            "Amanzimtoti", "Kingsburgh", "Umbogintwini", "Umkomaas", "Folweni",
            "Hillcrest", "Botha's Hill", "Shongweni", "Inchanga", "Pinetown",
            "Westville", "Clermont", "KwaDabeka"
        };

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", "mov", ".mkv" };

        //Initializes the controller with the necessary services for event, recommendation, and file operations.
        public EventController(EventService eventService, RecommendationService recommendationService, IWebHostEnvironment env)
        {
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public static class EventTypes
        {
            public const string Event = "Event";
            public const string Announcement = "Announcement";
        }

        //Checks if the currently logged-in user has the 'Admin' role based on session data.
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        //Checks if the currently logged-in user has the 'User' role based on session data.
        private bool IsUser() => HttpContext.Session.GetString("Role") == "User";

        //Determines if the uploaded file's extension is one of the allowed media types.
        private bool IsAllowedMedia(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }

        //Saves the uploaded media file to the wwwroot/uploads/events directory and returns the public path.
        private string SaveMediaFile(IFormFile media)
        {
            var uploadsRoot = Path.Combine(
                _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "uploads", "events");

            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(media.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using var fs = new FileStream(fullPath, FileMode.Create);
            media.CopyTo(fs);

            return $"/uploads/events/{fileName}";
        }

        //Deletes the media file from the file system given its public path.
        private void DeleteMediaFile(string? mediaPath)
        {
            if (string.IsNullOrWhiteSpace(mediaPath)) return;

            var trimmed = mediaPath.TrimStart('/');
            var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                                         trimmed.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        //Converts a comma-separated string of hashtags into a distinct list of formatted hashtags.
        private static List<string> ParseHashtags(string? hashtags)
        {
            if (string.IsNullOrWhiteSpace(hashtags)) return new List<string>();
            return hashtags
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(h => h.StartsWith("#") ? h : $"#{h}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        //Checks if an Event has passed its end date by more than 30 days, making it hidden for regular users. 
        //Announcements are exempt from this rule.
        private static bool EventHiddenForUser(Event ev)
        {
            if (!ev.Type.Equals(EventTypes.Event, StringComparison.OrdinalIgnoreCase)) return false;

            if (!ev.EndDate.HasValue) return false;

            var end = ev.EndDate.Value.Date;
            var today = DateTime.UtcNow.ToLocalTime().Date;

            return (today - end).TotalDays > 30;
        }

        //Checks if an Event is in the 30-day archival period (post-end date but not yet hidden). 
        //Announcements are exempt from this rule.
        private static bool EventIsArchivedForUser(Event ev)
        {
            if (!ev.Type.Equals(EventTypes.Event, StringComparison.OrdinalIgnoreCase)) return false;

            if (!ev.EndDate.HasValue) return false;
            var end = ev.EndDate.Value.Date;
            var today = DateTime.UtcNow.ToLocalTime().Date;
            if (end >= today) return false;
            return (today - end).TotalDays <= 30;
        }

        //Generates a deterministic GUID based on the logged-in username for use in tracking and recommendations.
        private Guid? GetLoggedInUserId()
        {
            var username = HttpContext?.Session.GetString("Username");
            if (string.IsNullOrWhiteSpace(username)) return null;

            var nameBytes = Encoding.UTF8.GetBytes(username.ToLowerInvariant());
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(nameBytes);

            try
            {
                return new Guid(hash);
            }
            catch
            {
                return null;
            }
        }

        //Displays a filtered list of active Events and Announcements for regular users. 
        //Redirects to AdminIndex if the user is an Admin.
        [HttpGet]
        public IActionResult Index(string? category, string? location, string? fromDate, string? toDate, string? endFrom, string? endTo, string? createdFrom, string? createdTo, string? hashtags, string? searchQuery, string? type)
        {
            if (IsAdmin()) return RedirectToAction("AdminIndex");

            DateOnly? from = null, to = null;
            if (!string.IsNullOrWhiteSpace(fromDate) && DateOnly.TryParse(fromDate, out var f)) from = f;
            if (!string.IsNullOrWhiteSpace(toDate) && DateOnly.TryParse(toDate, out var t)) to = t;

            DateOnly? endFromDate = null, endToDate = null;
            if (!string.IsNullOrWhiteSpace(endFrom) && DateOnly.TryParse(endFrom, out var ef)) endFromDate = ef;
            if (!string.IsNullOrWhiteSpace(endTo) && DateOnly.TryParse(endTo, out var et)) endToDate = et;

            DateOnly? createdFromDate = null, createdToDate = null;
            if (!string.IsNullOrWhiteSpace(createdFrom) && DateOnly.TryParse(createdFrom, out var cf)) createdFromDate = cf;
            if (!string.IsNullOrWhiteSpace(createdTo) && DateOnly.TryParse(createdTo, out var ct)) createdToDate = ct;

            Guid? userId = GetLoggedInUserId();
            _recommendationService.TrackSearch(userId, category, from, to, searchQuery: null, hashtag: null);
            var all = _eventService.GetAll();

            if (!string.IsNullOrWhiteSpace(type))
            {
                var filterType = type.Trim();
                if (filterType.Equals(EventTypes.Event, StringComparison.OrdinalIgnoreCase) ||
                    filterType.Equals(EventTypes.Announcement, StringComparison.OrdinalIgnoreCase))
                {
                    all = all.Where(e => !string.IsNullOrWhiteSpace(e.Type) && e.Type.Equals(filterType, StringComparison.OrdinalIgnoreCase)).ToList();
                }
            }

            all = all.Where(e => !EventHiddenForUser(e)).ToList();

            if (!string.IsNullOrWhiteSpace(location)) all = all.Where(e => !string.IsNullOrWhiteSpace(e.Location) && e.Location.Contains(location.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(category)) all = all.Where(e => !string.IsNullOrWhiteSpace(e.Category) && e.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (from.HasValue) all = all.Where(e => DateOnly.FromDateTime(e.StartDate.ToLocalTime()).CompareTo(from.Value) >= 0).ToList();
            if (to.HasValue) all = all.Where(e => DateOnly.FromDateTime(e.StartDate.ToLocalTime()).CompareTo(to.Value) <= 0).ToList();
            if (endFromDate.HasValue) all = all.Where(e => e.EndDate.HasValue && DateOnly.FromDateTime(e.EndDate.Value.ToLocalTime()).CompareTo(endFromDate.Value) >= 0).ToList();
            if (endToDate.HasValue) all = all.Where(e => e.EndDate.HasValue && DateOnly.FromDateTime(e.EndDate.Value.ToLocalTime()).CompareTo(endToDate.Value) <= 0).ToList();
            if (createdFromDate.HasValue) all = all.Where(e => DateOnly.FromDateTime(e.Timestamp.ToLocalTime()).CompareTo(createdFromDate.Value) >= 0).ToList();
            if (createdToDate.HasValue) all = all.Where(e => DateOnly.FromDateTime(e.Timestamp.ToLocalTime()).CompareTo(createdToDate.Value) <= 0).ToList();

            if (!string.IsNullOrWhiteSpace(hashtags))
            {
                var tags = ParseHashtags(hashtags);
                all = all.Where(e => e.Hashtags != null && e.Hashtags.Any(h => tags.Any(t => string.Equals(t, h, StringComparison.OrdinalIgnoreCase)))).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim();
                all = all.Where(e => (!string.IsNullOrWhiteSpace(e.Title) && e.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                                     (!string.IsNullOrWhiteSpace(e.Location) && e.Location.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                                     (!string.IsNullOrWhiteSpace(e.Category) && e.Category.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                                     (!string.IsNullOrWhiteSpace(e.Description) && e.Description.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                                     (e.Hashtags != null && e.Hashtags.Any(h => h.Contains(q, StringComparison.OrdinalIgnoreCase)))
                                     ).ToList();
            }

            var active = all.Where(e => !EventIsArchivedForUser(e)).ToList();

            var recommendations = _recommendationService.GetRecommendations(userId, 5);

            ViewBag.Categories = _eventService.GetUniqueCategories();
            ViewBag.Locations = EventController.DefaultLocations;
            ViewBag.Dates = _eventService.GetUniqueEventDates();
            ViewBag.Recommendations = recommendations;
            ViewBag.Filters = new { category, fromDate, toDate, endFrom, endTo, location, createdFrom, createdTo, hashtags, searchQuery, type };
            return View("Index", active);
        }

        //Displays a filtered list of active Events and Announcements containing a specific hashtag.
        //This action is intended to be the target for hashtag clicks on the Index/Details views.
        [HttpGet]
        public IActionResult HashTags(string? hashtag)
        {
            if (IsAdmin()) return RedirectToAction("AdminIndex");

            // 1. Get all events/announcements
            var all = _eventService.GetAll();

            // 2. Filter out hidden events (past 30 days) and announcements
            all = all.Where(e => !EventHiddenForUser(e)).ToList();

            if (!string.IsNullOrWhiteSpace(hashtag))
            {
                // 3. Parse the input hashtag for consistent filtering (e.g., adds # if missing)
                var tagsToSearch = ParseHashtags(hashtag);

                // Ensure we only proceed if we have a valid tag to search for
                if (tagsToSearch.Any())
                {
                    // 4. Filter events that contain any of the parsed tags (usually just one)
                    all = all.Where(e => e.Hashtags != null &&
                                         e.Hashtags.Any(eventTag => tagsToSearch.Any(searchTag => string.Equals(eventTag, searchTag, StringComparison.OrdinalIgnoreCase))))
                             .ToList();
                }
            }

            // 5. Filter out archived events (keeping only currently active or non-archived ones)
            var active = all.Where(e => !EventIsArchivedForUser(e)).ToList();

            // 6. Set up ViewBag for the view
            ViewBag.Categories = _eventService.GetUniqueCategories();
            ViewBag.Locations = EventController.DefaultLocations;
            ViewBag.CurrentHashtag = hashtag?.StartsWith("#") == true ? hashtag : $"#{hashtag}"; // Pass the hashtag for the view title/context

            // 7. Return the view HashTags.cshtml
            return View("HashTags", active);
        }

        //Displays a filtered list of active Announcements for regular users.
        [HttpGet]
        public IActionResult Announcements(string? category, string? location, string? createdFrom, string? createdTo,
                                 string? hashtags, string? searchQuery)
        {
            if (IsAdmin()) return RedirectToAction("AdminIndex");

            DateOnly? createdFromDate = null, createdToDate = null;
            if (!string.IsNullOrWhiteSpace(createdFrom) && DateOnly.TryParse(createdFrom, out var cf)) createdFromDate = cf;
            if (!string.IsNullOrWhiteSpace(createdTo) && DateOnly.TryParse(createdTo, out var ct)) createdToDate = ct;

            Guid? userId = GetLoggedInUserId();

            _recommendationService.TrackSearch(userId, category, null, null, searchQuery: null, hashtag: null);

            var all = _eventService.GetAll();

            all = all.Where(e => !string.IsNullOrWhiteSpace(e.Type) &&
                e.Type.Equals(EventTypes.Announcement, StringComparison.OrdinalIgnoreCase))
            .ToList();

            if (!string.IsNullOrWhiteSpace(location))
                all = all.Where(e => !string.IsNullOrWhiteSpace(e.Location) &&
                                     e.Location.Contains(location.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(category))
                all = all.Where(e => !string.IsNullOrWhiteSpace(e.Category) &&
                                     e.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (createdFromDate.HasValue)
                all = all.Where(e => DateOnly.FromDateTime(e.Timestamp.ToLocalTime()).CompareTo(createdFromDate.Value) >= 0).ToList();

            if (createdToDate.HasValue)
                all = all.Where(e => DateOnly.FromDateTime(e.Timestamp.ToLocalTime()).CompareTo(createdToDate.Value) <= 0).ToList();

            if (!string.IsNullOrWhiteSpace(hashtags))
            {
                var tags = ParseHashtags(hashtags);
                all = all.Where(e => e.Hashtags != null && e.Hashtags.Any(h => tags.Any(t => string.Equals(t, h, StringComparison.OrdinalIgnoreCase)))).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim();
                all = all.Where(e =>
                    (!string.IsNullOrWhiteSpace(e.Title) && e.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Location) && e.Location.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Category) && e.Category.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Description) && e.Description.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Hashtags != null && e.Hashtags.Any(h => h.Contains(q, StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }

            all = all.OrderByDescending(e => e.Timestamp).ToList();

            ViewBag.Categories = _eventService.GetUniqueCategories();
            ViewBag.Locations = EventController.DefaultLocations;
            ViewBag.Filters = new { category, location, createdFrom, createdTo, hashtags, searchQuery };

            return View("AnnouncementsIndex", all);
        }

        //Displays a filtered list of archived Events (past 30 days but not yet hidden) for regular users. 
        //Announcements are excluded.
        [HttpGet]
        public IActionResult Archived(string? category, string? location, string? fromDate, string? toDate,
                                 string? endFrom, string? endTo,
                                 string? hashtags, string? searchQuery, string? orderByEndDate)
        {
            if (IsAdmin()) return RedirectToAction("AdminIndex");

            var all = _eventService.GetAll();

            var events = all.Where(e => !string.IsNullOrWhiteSpace(e.Type) &&
                                     e.Type.Equals(EventTypes.Event, StringComparison.OrdinalIgnoreCase) &&
                                     EventIsArchivedForUser(e)).ToList();

            if (!string.IsNullOrWhiteSpace(category))
                events = events.Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(location))
                events = events.Where(e => string.Equals(e.Location, location, StringComparison.OrdinalIgnoreCase)).ToList();

            if (DateOnly.TryParse(fromDate, out var sFrom))
                events = events.Where(e => DateOnly.FromDateTime(e.StartDate.ToLocalTime()).CompareTo(sFrom) >= 0).ToList();

            if (DateOnly.TryParse(toDate, out var sTo))
                events = events.Where(e => DateOnly.FromDateTime(e.StartDate.ToLocalTime()).CompareTo(sTo) <= 0).ToList();

            if (DateOnly.TryParse(endFrom, out var eFrom))
                events = events.Where(e => e.EndDate.HasValue && DateOnly.FromDateTime(e.EndDate.Value.ToLocalTime()).CompareTo(eFrom) >= 0).ToList();

            if (DateOnly.TryParse(endTo, out var eTo))
                events = events.Where(e => e.EndDate.HasValue && DateOnly.FromDateTime(e.EndDate.Value.ToLocalTime()).CompareTo(eTo) <= 0).ToList();

            var searchHashtags = ParseHashtags(hashtags);
            if (searchHashtags.Any())
                events = events.Where(e => e.Hashtags != null &&
                                         e.Hashtags.Intersect(searchHashtags, StringComparer.OrdinalIgnoreCase).Any()).ToList();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim();
                events = events.Where(e =>
                    (!string.IsNullOrWhiteSpace(e.Title) && e.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Description) && e.Description.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Category) && e.Category.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Location) && e.Location.Contains(q, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            events = orderByEndDate?.ToLower() switch
            {
                "asc" => events.OrderBy(e => e.EndDate).ToList(),
                "desc" => events.OrderByDescending(e => e.EndDate).ToList(),
                _ => events.OrderByDescending(e => e.EndDate).ToList()
            };

            ViewBag.Filters = new { category, location, fromDate, toDate, endFrom, endTo, hashtags, searchQuery, orderByEndDate };

            ViewBag.Categories = _eventService.GetUniqueCategories();
            ViewBag.Locations = DefaultLocations;

            ViewBag.ArchivedNotice = "These events have ended and will no longer be available to view after 30 days from their end date.";

            return View("Archived", events);
        }

        //Displays the full details for a specific Event or Announcement, provided it is not hidden from the user.
        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var ev = _eventService.GetById(id);
            if (ev == null) return NotFound();

            if (EventHiddenForUser(ev) && !IsAdmin())
                return NotFound();

            return View("Details", ev);
        }

        // ============ ADMIN SIDE ============

        //Displays a complete, unfiltered/filterable list of all Events and Announcements for administrators.
        [HttpGet]
        public IActionResult AdminIndex(string? category, string? fromDate, string? toDate,
                                 string? endFrom, string? endTo,
                                 string? location, string? createdFrom, string? createdTo,
                                 string? hashtags, string? searchQuery, string? sortOrder,
                                 string? type)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            DateOnly? from = null, to = null;
            if (!string.IsNullOrWhiteSpace(fromDate) && DateOnly.TryParse(fromDate, out var f)) from = f;
            if (!string.IsNullOrWhiteSpace(toDate) && DateOnly.TryParse(toDate, out var t)) to = t;

            DateOnly? endFromDate = null, endToDate = null;
            if (!string.IsNullOrWhiteSpace(endFrom) && DateOnly.TryParse(endFrom, out var ef)) endFromDate = ef;
            if (!string.IsNullOrWhiteSpace(endTo) && DateOnly.TryParse(endTo, out var et)) endToDate = et;

            DateOnly? createdFromDate = null, createdToDate = null;
            if (!string.IsNullOrWhiteSpace(createdFrom) && DateOnly.TryParse(createdFrom, out var cf)) createdFromDate = cf;
            if (!string.IsNullOrWhiteSpace(createdTo) && DateOnly.TryParse(createdTo, out var ct)) createdToDate = ct;

            var all = _eventService.GetAll();

            if (!string.IsNullOrWhiteSpace(type) &&
        (type.Equals(EventTypes.Event, StringComparison.OrdinalIgnoreCase) ||
        type.Equals(EventTypes.Announcement, StringComparison.OrdinalIgnoreCase)))
            {
                all = all.Where(e => !string.IsNullOrWhiteSpace(e.Type) &&
                                     e.Type.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(location))
                all = all.Where(e => !string.IsNullOrWhiteSpace(e.Location) &&
                                     e.Location.Contains(location.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(category))
                all = all.Where(e => !string.IsNullOrWhiteSpace(e.Category) &&
                                     e.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (from.HasValue)
                all = all.Where(e => DateOnly.FromDateTime(e.StartDate.ToLocalTime()).CompareTo(from.Value) >= 0).ToList();

            if (to.HasValue)
                all = all.Where(e => DateOnly.FromDateTime(e.StartDate.ToLocalTime()).CompareTo(to.Value) <= 0).ToList();

            if (endFromDate.HasValue)
                all = all.Where(e => e.EndDate.HasValue && DateOnly.FromDateTime(e.EndDate.Value.ToLocalTime()).CompareTo(endFromDate.Value) >= 0).ToList();

            if (endToDate.HasValue)
                all = all.Where(e => e.EndDate.HasValue && DateOnly.FromDateTime(e.EndDate.Value.ToLocalTime()).CompareTo(endToDate.Value) <= 0).ToList();


            if (createdFromDate.HasValue)
                all = all.Where(e => DateOnly.FromDateTime(e.Timestamp.ToLocalTime()).CompareTo(createdFromDate.Value) >= 0).ToList();

            if (createdToDate.HasValue)
                all = all.Where(e => DateOnly.FromDateTime(e.Timestamp.ToLocalTime()).CompareTo(createdToDate.Value) <= 0).ToList();

            if (!string.IsNullOrWhiteSpace(hashtags))
            {
                var tags = ParseHashtags(hashtags);
                all = all.Where(e => e.Hashtags != null && e.Hashtags.Any(h => tags.Any(t => string.Equals(t, h, StringComparison.OrdinalIgnoreCase)))).ToList();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.Trim();
                all = all.Where(e =>
                    (!string.IsNullOrWhiteSpace(e.Title) && e.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Location) && e.Location.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(e.Category) && e.Category.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Hashtags != null && e.Hashtags.Any(h => h.Contains(q, StringComparison.OrdinalIgnoreCase)))
                ).ToList();
            }

            all = sortOrder switch
            {
                "start_asc" => all.OrderBy(e => e.StartDate).ToList(),
                "start_desc" => all.OrderByDescending(e => e.StartDate).ToList(),
                "end_asc" => all.OrderBy(e => e.EndDate).ToList(),
                "end_desc" => all.OrderByDescending(e => e.EndDate).ToList(),
                "created_asc" => all.OrderBy(e => e.Timestamp).ToList(),
                "created_desc" => all.OrderByDescending(e => e.Timestamp).ToList(),
                _ => all.OrderBy(e => e.StartDate).ToList()
            };

            ViewBag.Categories = DefaultCategories;
            ViewBag.Locations = DefaultLocations;
            ViewBag.Dates = _eventService.GetUniqueEventDates();
            ViewBag.EventTypes = new List<string> { EventTypes.Event, EventTypes.Announcement };

            ViewBag.Filters = new
            {
                category,
                location,
                fromDate,
                toDate,
                endFrom,
                endTo,
                createdFrom,
                createdTo,
                hashtags,
                searchQuery,
                sortOrder,
                type
            };

            return View("AdminIndex", all);
        }


        //Displays the form for creating a new Event or Announcement (Admin only).
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            ViewBag.Categories = DefaultCategories;
            ViewBag.Locations = DefaultLocations;
            ViewBag.EventTypes = new List<string> { EventTypes.Event, EventTypes.Announcement };

            return View(new Event());
        }


        //Handles the POST submission to create a new Event or Announcement, 
        //including media upload and hashtag parsing (Admin only).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Event model, IFormFile? media, string? hashtags)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            if (media != null && media.Length > 0 && !IsAllowedMedia(media))
            {
                ModelState.AddModelError("MediaPath", "Invalid media format. Allowed: JPG, JPEG, PNG, GIF, MP4, AVI, MOV, MKV.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = DefaultCategories;
                ViewBag.Locations = DefaultLocations;
                ViewBag.EventTypes = new List<string> { EventTypes.Event, EventTypes.Announcement };
                return View(model);
            }

            if (media != null && media.Length > 0)
            {
                model.MediaPath = SaveMediaFile(media);
            }

            model.Hashtags = ParseHashtags(hashtags);

            if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();
            if (model.Timestamp == default) model.Timestamp = DateTime.UtcNow;
            model.ShareUrl = Url.Action("Details", "Event", new { id = model.Id }, Request.Scheme) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(model.Type))
                model.Type = EventTypes.Event;

            _eventService.Create(model);

            TempData["Message"] = "Event created.";
            return RedirectToAction("AdminIndex");
        }


        //Displays the form for editing an existing Event or Announcement (Admin only).
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var ev = _eventService.GetById(id);
            if (ev == null) return NotFound();

            ViewBag.Categories = DefaultCategories;
            ViewBag.Locations = DefaultLocations;
            ViewBag.EventTypes = new List<string> { EventTypes.Event, EventTypes.Announcement };

            return View(ev);
        }

        //Handles the POST submission to update an existing Event or Announcement, 
        //managing media replacement/deletion (Admin only).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, Event model, IFormFile? media, string? hashtags)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var ev = _eventService.GetById(id);
            if (ev == null) return NotFound();

            if (media != null && media.Length > 0)
            {
                if (!IsAllowedMedia(media))
                {
                    ModelState.AddModelError("MediaPath", "Invalid media format. Allowed: JPG, JPEG, PNG, GIF, MP4, AVI, MOV, MKV.");
                    ViewBag.Categories = DefaultCategories;
                    ViewBag.Locations = DefaultLocations;
                    ViewBag.EventTypes = new List<string> { EventTypes.Event, EventTypes.Announcement };
                    return View(model);
                }

                if (!string.IsNullOrEmpty(ev.MediaPath))
                    DeleteMediaFile(ev.MediaPath);

                ev.MediaPath = SaveMediaFile(media);
            }

            ev.Title = model.Title.Trim();
            ev.Description = model.Description.Trim();
            ev.Location = model.Location?.Trim() ?? string.Empty;
            ev.Category = model.Category.Trim();
            ev.StartDate = model.StartDate;
            ev.EndDate = model.EndDate;
            ev.Hashtags = ParseHashtags(hashtags);
            ev.Type = model.Type.Trim();

            ev.ShareUrl = Url.Action("Details", "Event", new { id = ev.Id }, Request.Scheme) ?? ev.ShareUrl;

            _eventService.Update(ev);
            TempData["Message"] = "Event updated.";
            return RedirectToAction("AdminIndex");
        }


        //Handles the POST submission to delete a specific Event or Announcement and its associated media file (Admin only).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var ev = _eventService.GetById(id);
            if (ev == null) return NotFound();

            if (!string.IsNullOrEmpty(ev.MediaPath))
                DeleteMediaFile(ev.MediaPath);

            _eventService.Delete(id);
            TempData["Message"] = "Event deleted.";
            return RedirectToAction("AdminIndex");
        }

        //Displays the full details for a specific Event or Announcement, typically used for an administrative or unfiltered view.
        [HttpGet]
        public IActionResult AdminDetails(Guid id)
        {
            var ev = _eventService.GetById(id);
            if (ev == null) return NotFound();
            return View("AdminDetails", ev);
        }
    }
}