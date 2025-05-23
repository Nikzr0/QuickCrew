using Microsoft.AspNetCore.Mvc;
using QuickCrew.Shared.Models;
using QuickCrew.Web.Models;
using System.Diagnostics;
using System.Net.Http;

namespace QuickCrew.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(
            ILogger<HomeController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("QuickCrewAPI");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch stats from API
                var jobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>("api/job-postings");
                var stats = await _httpClient.GetFromJsonAsync<PlatformStatsDto>("api/Stats");

                ViewBag.ActiveJobs = jobs?.Count ?? 0;
                ViewBag.RegisteredUsers = stats?.TotalUsers ?? 0;
                ViewBag.ActiveProjects = stats?.ActiveProjects ?? 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");

                ViewBag.ActiveJobs = 0;
                ViewBag.RegisteredUsers = 0;
                ViewBag.ActiveProjects = 0;

                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class JobPostingsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JobPostingsController> _logger;

        public JobPostingsController(
            IHttpClientFactory httpClientFactory,
            ILogger<JobPostingsController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("QuickCrewAPI");
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var jobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>("api/job-postings");
                return View(jobs ?? new List<JobPostingDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading job postings");
                return View(new List<JobPostingDto>());
            }
        }
    }

    public class PlatformStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveProjects { get; set; }
    }
}