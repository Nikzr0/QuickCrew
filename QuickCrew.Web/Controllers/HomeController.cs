using Microsoft.AspNetCore.Mvc;
using QuickCrew.Shared.Models;
using QuickCrew.Web.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace QuickCrew.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7224/");
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            try
            {
                var pagedJobs = await _httpClient.GetFromJsonAsync<PagedResult<JobPostingDto>>("api/job-postings?pageNumber=1&pageSize=5");

                if (pagedJobs != null && pagedJobs.Items != null)
                {
                    viewModel.RecentJobPostings = pagedJobs.Items;
                    viewModel.ActiveJobs = pagedJobs.TotalCount;
                }
                else
                {
                    _logger.LogWarning("API returned null or empty PagedResult for recent job postings on dashboard.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent job postings for dashboard.");
            }

            return View(viewModel);
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
}