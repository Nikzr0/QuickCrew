using Microsoft.AspNetCore.Mvc;
using QuickCrew.Shared.Models;
using QuickCrew.Web.Models;
using System.Diagnostics;

namespace QuickCrew.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
        
        public class JobPostingsController : Controller
        {
            private readonly HttpClient _httpClient;

            public JobPostingsController(IHttpClientFactory httpClientFactory)
            {
                _httpClient = httpClientFactory.CreateClient("ApiClient");
            }

            public async Task<IActionResult> Index()
            {
                var jobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>("api/JobPostings");
                return View(jobs);
            }
        }
    }
}
