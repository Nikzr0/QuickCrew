using Microsoft.AspNetCore.Mvc;
using QuickCrew.Shared.Models;
using System.Net.Http;

namespace QuickCrew.Web.Controllers
{
    public class JobPostsController : Controller
    {
        private readonly HttpClient _httpClient;

        public JobPostsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("QuickCrewAPI");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Job-Postings");

                if (!response.IsSuccessStatusCode)
                {
                    return View(new List<JobPostingDto>());
                }

                var jobs = await response.Content.ReadFromJsonAsync<List<JobPostingDto>>();
                return View(jobs ?? new List<JobPostingDto>());
            }
            catch (Exception ex)
            {
                // Log error here
                return View(new List<JobPostingDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var job = await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/JobPostings/{id}");
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }
    }
}