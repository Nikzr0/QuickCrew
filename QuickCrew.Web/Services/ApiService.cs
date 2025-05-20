using QuickCrew.Shared.Models; // Референция към DTOs

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7224"); // API адрес
    }

    // GET Locations
    public async Task<List<LocationDto>> GetLocationsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<LocationDto>>("/api/locations");
    }

    // POST Location
    public async Task<LocationDto> AddLocationAsync(LocationDto dto)
    { var response = await _httpClient.PostAsJsonAsync("/api/locations", dto);
        return await response.Content.ReadFromJsonAsync<LocationDto>();
    }
}