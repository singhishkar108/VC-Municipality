using Microsoft.AspNetCore.Mvc;
using MunicipalityApp.Models;
using MunicipalityApp.Helpers;
using MunicipalityApp.Services;

namespace MunicipalityApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly FileUserService _userService;

        public AuthController(FileUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (!_userService.Register(username, password))
            {
                TempData["ErrorMessage"] = "Username already exists.";
                return View();
            }

            // Use a unique TempData key for the registration success message
            TempData["RegistrationSuccessMessage"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _userService.Login(username, password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                TempData["SuccessMessage"] = "Login successful!";
                return RedirectToAction("Index", "Home");
            }

            TempData["FormError"] = "Incorrect username or password.";
            TempData["LoginErrorToastr"] = "Incorrect username or password.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["LogoutMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "Auth");
        }
    }
}