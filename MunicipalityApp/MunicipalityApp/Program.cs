using Microsoft.AspNetCore.Localization;
using MunicipalityApp.Services;
using System.Globalization;
using Microsoft.AspNetCore.Mvc; // Add this line if it's not present (required for MvcOptions)

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// FIX: Enforce Invariant Culture for Model Binding
// ----------------------------------------------------

// Set the application's default culture to 'en-US' (or InvariantCulture) 
// to ensure that decimal separators are consistently parsed using the dot ('.').
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// --------------------
// Service Configuration
// --------------------

// Allow access to HttpContext in controllers/views
builder.Services.AddHttpContextAccessor();

// Distributed cache (required for session state)
builder.Services.AddDistributedMemoryCache();

// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout duration
    options.Cookie.HttpOnly = true;                 // Session cookie accessible only by the server
    options.Cookie.IsEssential = true;              // Required for the app to function
});

// Localization setup
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    // --- ADDED: Configure MvcOptions to override decimal parsing error message ---
    .AddMvcOptions(options =>
    {
        // This provides a specific, helpful error message to the user
        // guiding them to use a dot ('.') as the decimal separator.
        options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
            (s) => $"The field {s} must be a valid number. Please use a dot ('.') as the decimal separator (e.g., 34.00000).");
    })
    // ----------------------------------------------------------------------------
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// --------------------
// Custom Application Services
// --------------------

// These are registered as singletons because they load data from files and 
// maintain in-memory structures that should persist during the app's lifetime.

builder.Services.AddSingleton<FileUserService>();
builder.Services.AddSingleton<BlogPostService>();
builder.Services.AddSingleton<IssueService>();

// Order matters only for clarity — RecommendationService depends on EventService
builder.Services.AddSingleton<EventService>();
builder.Services.AddSingleton<RecommendationService>();

builder.Services.AddSingleton<ServiceRequestService>();

// --------------------
// Build Application
// --------------------

var app = builder.Build();

// --------------------
// Middleware Pipeline
// --------------------

// Supported cultures for localization
var supportedCultures = new[]
{
    new CultureInfo("en"), // English
    new CultureInfo("af"), // Afrikaans
    new CultureInfo("zu"), // isiZulu
};

var localizationOptions = new RequestLocalizationOptions
{
    // NOTE: Setting the DefaultRequestCulture to "en" here helps, 
    // but the FIX above (setting DefaultThreadCurrentCulture) is the key
    // to overriding the internal model binding behavior.
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Default MVC route mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --------------------
// Run Application
// --------------------
app.Run();