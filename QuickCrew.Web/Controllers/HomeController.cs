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
            var viewModel = new DashboardViewModel();

            try
            {
                // Fetch job postings (all, then we'll take top N for "recent")
                var jobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>("api/job-postings");

                // Fetch stats from API (if you have /api/Stats endpoint)
                PlatformStatsDto stats = null;
                try
                {
                    stats = await _httpClient.GetFromJsonAsync<PlatformStatsDto>("api/Stats");
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "Could not fetch platform stats from API. API endpoint /api/Stats might be missing or returned an error.");
                    // Set default values if stats API is not available
                    stats = new PlatformStatsDto { TotalUsers = 0, ActiveProjects = 0 };
                }


                viewModel.ActiveJobs = jobs?.Count ?? 0;
                viewModel.RegisteredUsers = stats?.TotalUsers ?? 0;
                viewModel.ActiveProjects = stats?.ActiveProjects ?? 0;

                viewModel.RecentJobPostings = jobs?
                    .OrderByDescending(j => j.CreatedDate)
                    .Take(5)
                    .ToList() ?? new List<JobPostingDto>();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");

                viewModel.ActiveJobs = 0;
                viewModel.RegisteredUsers = 0;
                viewModel.ActiveProjects = 0;
                viewModel.RecentJobPostings = new List<JobPostingDto>();

                return View(viewModel);
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
    
    //public class PlatformStatsDto
    //{
    //    public int TotalUsers { get; set; }
    //    public int ActiveProjects { get; set; }
    //}
}