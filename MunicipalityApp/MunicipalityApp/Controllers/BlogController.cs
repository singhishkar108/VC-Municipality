using Microsoft.AspNetCore.Mvc;
using MunicipalityApp.Models;
using MunicipalityApp.Services;

namespace MunicipalityApp.Controllers
{
    public class BlogController : Controller
    {
        private readonly BlogPostService _service;
        private readonly IWebHostEnvironment _env;

        // Allowed media extensions
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov", ".mkv" };

        public BlogController(BlogPostService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsUser() => HttpContext.Session.GetString("Role") == "User";

        private bool IsAllowedMedia(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }

        // ============ USER SIDE ============
        [HttpGet]
        public IActionResult Feed(string? fromDate, string? toDate)
        {
            if (IsAdmin()) return RedirectToAction("Index");

            DateOnly? from = null, to = null;
            if (!string.IsNullOrWhiteSpace(fromDate) && DateOnly.TryParse(fromDate, out var f)) from = f;
            if (!string.IsNullOrWhiteSpace(toDate) && DateOnly.TryParse(toDate, out var t)) to = t;

            var posts = _service.SearchByDate(from, to);
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            return View("Feed", posts);
        }

        [HttpGet]
        public IActionResult Grid(string? fromDate, string? toDate)
        {
            if (IsAdmin()) return RedirectToAction("Index");

            DateOnly? from = null, to = null;
            if (!string.IsNullOrWhiteSpace(fromDate) && DateOnly.TryParse(fromDate, out var f)) from = f;
            if (!string.IsNullOrWhiteSpace(toDate) && DateOnly.TryParse(toDate, out var t)) to = t;

            var posts = _service.SearchByDate(from, to);
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View("Grid", posts);
        }

        [HttpGet]
        public IActionResult ViewPost(Guid id)
        {
            var post = _service.GetById(id);
            if (post == null) return NotFound();
            return View("ViewPost", post);
        }

        [HttpPost]
        public IActionResult Like(Guid id, string? returnUrl)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Auth");
            }

            _service.LikePost(id, username);

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Feed"); // fallback
        }

        [HttpPost]
        public IActionResult Unlike(Guid id, string? returnUrl)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Auth");
            }

            _service.UnlikePost(id, username);

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Feed"); // fallback
        }

        // ============ ADMIN SIDE ============
        public IActionResult Index(string? fromDate, string? toDate, string? searchQuery, string? sortOrder)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            DateOnly? from = null, to = null;
            if (!string.IsNullOrWhiteSpace(fromDate) && DateOnly.TryParse(fromDate, out var f)) from = f;
            if (!string.IsNullOrWhiteSpace(toDate) && DateOnly.TryParse(toDate, out var t)) to = t;

            var posts = _service.SearchByDate(from, to);

            // Filter by search query (title, hashtags, location)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var lowerQuery = searchQuery.ToLower();
                posts = posts.Where(p =>
                    (p.Title?.ToLower().Contains(lowerQuery) ?? false) ||
                    (p.Location?.ToLower().Contains(lowerQuery) ?? false) ||
                    (p.Hashtags?.Any(h => h.ToLower().Contains(lowerQuery)) ?? false)
                ).ToList();
            }

            // Sort by likes
            posts = sortOrder switch
            {
                "likes_asc" => posts.OrderBy(p => p.Likes).ToList(),
                "likes_desc" => posts.OrderByDescending(p => p.Likes).ToList(),
                _ => posts
            };

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SortOrder = sortOrder;

            return View("Index", posts);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View(new BlogPost());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BlogPost model, IFormFile? media, string? hashtags)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            // Validate media
            if (media == null || media.Length == 0)
            {
                ModelState.AddModelError("MediaPath", "Media file is required.");
            }
            else if (!IsAllowedMedia(media))
            {
                ModelState.AddModelError("MediaPath", "Invalid media format. Allowed: JPG, JPEG, PNG, GIF, MP4, AVI, MOV, MKV.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = HttpContext.Session.GetString("Username") ?? "Admin";
            string mediaPath = SaveMediaFile(media!);

            var post = new BlogPost
            {
                Title = model.Title.Trim(),
                Username = username,
                MediaPath = mediaPath,
                Description = model.Description.Trim(),
                Hashtags = ParseHashtags(hashtags),
                Location = model.Location.Trim(),
                Likes = 0,
                Timestamp = DateTime.UtcNow.AddHours(2),
                ShareUrl = Url.Action("ViewPost", "Blog", new { id = model.Id }, Request.Scheme) ?? string.Empty
            };

            _service.Create(post);
            TempData["Message"] = "Post created.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var post = _service.GetById(id);
            if (post == null) return NotFound();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, BlogPost model, IFormFile? media, string? hashtags)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var post = _service.GetById(id);
            if (post == null) return NotFound();

            // Validate new media if uploaded
            if (media != null && media.Length > 0)
            {
                if (!IsAllowedMedia(media))
                {
                    ModelState.AddModelError("MediaPath", "Invalid media format. Allowed: JPG, JPEG, PNG, GIF, MP4, AVI, MOV, MKV.");
                    return View(model);
                }

                // Delete old file if exists
                if (!string.IsNullOrEmpty(post.MediaPath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                                               post.MediaPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                post.MediaPath = SaveMediaFile(media);
            }

            post.Title = model.Title.Trim();
            post.Description = model.Description.Trim();
            post.Location = model.Location.Trim();
            post.Hashtags = ParseHashtags(hashtags);

            _service.Update(post);
            TempData["Message"] = "Post updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Guid id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            var post = _service.GetById(id);
            if (post == null) return NotFound();

            if (!string.IsNullOrEmpty(post.MediaPath))
            {
                var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                                            post.MediaPath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            _service.Delete(id);
            TempData["Message"] = "Post deleted.";
            return RedirectToAction("Index");
        }

        // ============ Helpers ============
        private string SaveMediaFile(IFormFile media)
        {
            var uploadsRoot = Path.Combine(
                _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "uploads", "posts");

            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(media.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using var fs = new FileStream(fullPath, FileMode.Create);
            media.CopyTo(fs);

            return $"/uploads/posts/{fileName}";
        }

        private static List<string> ParseHashtags(string? hashtags)
        {
            if (string.IsNullOrWhiteSpace(hashtags)) return new List<string>();
            return hashtags
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(h => h.StartsWith("#") ? h : $"#{h}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
