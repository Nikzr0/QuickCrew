using Microsoft.AspNetCore.Mvc;
using QuickCrew.Shared.Models;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 12)
        {
            try
            {
                var apiUrl = $"api/job-postings?pageNumber={pageNumber}&pageSize={pageSize}";
                var pagedJobs = await _httpClient.GetFromJsonAsync<PagedResult<JobPostingDto>>(apiUrl);

                if (pagedJobs == null)
                {
                    pagedJobs = new PagedResult<JobPostingDto>();
                }

                return View(pagedJobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading paged job postings for JobPostsController Index.");
                return View(new PagedResult<JobPostingDto>());
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

        [Authorize]
        public async Task<IActionResult> MyJobs()
        {
            try
            {
                var myJobPostings = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>("api/job-postings/my");

                if (myJobPostings == null)
                {
                    myJobPostings = new List<JobPostingDto>();
                }

                return View(myJobPostings);
            }
            catch (HttpRequestException httpEx) when (httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized access to MyJobs. User not authenticated or token invalid.");
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading 'My Jobs' for JobPostsController.");
                return View(new List<JobPostingDto>());
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPostingDto newJob)
        {
            if (!ModelState.IsValid)
            {
                return View(newJob);
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/job-postings", newJob);
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(MyJobs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new job posting.");
                ModelState.AddModelError("", "Could not create job posting. Please try again.");
                return View(newJob);
            }
        }

        //[HttpPost]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(JobPostingDto newJob)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(newJob);
        //    }

        //    try
        //    {
        //        var response = await _httpClient.PostAsJsonAsync("api/job-postings", newJob);
        //        response.EnsureSuccessStatusCode();

        //        return RedirectToAction(nameof(MyJobs));
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