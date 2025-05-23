using Microsoft.AspNetCore.Mvc;
using QuickCrew.Shared.Models;
using System.Net.Http;
using QuickCrew.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace QuickCrew.Web.Controllers
{
    public class JobPostsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;

        public JobPostsController(
            IHttpClientFactory httpClientFactory,
            IMapper mapper)
        {
            _httpClient = httpClientFactory.CreateClient("QuickCrewAPI");
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var jobs = await _httpClient.GetFromJsonAsync<List<JobPostingDto>>("api/job-postings");
            return View(jobs ?? new List<JobPostingDto>());
        }

        public async Task<IActionResult> Details(int id)
        {
            var job = await _httpClient.GetFromJsonAsync<JobPostingDto>($"api/job-postings/{id}");
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }
    }
}