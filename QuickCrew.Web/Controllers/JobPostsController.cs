using Microsoft.AspNetCore.Mvc;
using QuickCrew.Shared.Models;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QuickCrew.Web.Controllers
{
    public class JobPostsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JobPostsController> _logger;

        public JobPostsController(
            IHttpClientFactory httpClientFactory,
            ILogger<JobPostsController> logger)
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
                _logger.LogError(ex, "Error loading job postings for JobPostsController Index.");
                return View(new List<JobPostingDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var job = await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/job-postings/{id}");
                if (job == null)
                {
                    _logger.LogWarning($"Job posting with ID {id} not found.");
                    return NotFound();
                }
                return View(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading job posting details for ID: {id}");
                return NotFound();
            }
        }

        //[HttpGet]
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> Create(JobPostingDto newJob)
        //{
        //    try
        //    {
        //        var response = await _httpClient.PostAsJsonAsync("api/job-postings", newJob);
        //        response.EnsureSuccessStatusCode(); // Throws if not 2xx
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating new job posting.");
        //        ModelState.AddModelError("", "Could not create job posting. Please try again.");
        //        return View(newJob);
        //    }
        //}
    }
}