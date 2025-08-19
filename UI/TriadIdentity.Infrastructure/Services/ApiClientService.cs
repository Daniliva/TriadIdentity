using System.Net.Http.Json;
using TriadIdentity.BLL.Models.Identity;

namespace TriadIdentity.Infrastructure.Services
{
    public class ApiClientService
    {
        private readonly HttpClient _httpClient;

        public ApiClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task RegisterAsync(RegisterUserDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/identity/register", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> LoginAsync(LoginUserDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/identity/login", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task UpdateUserAsync(UpdateUserDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync("api/identity/update", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<UpdateUserDto> GetProfileAsync(Guid userId)
        {
            return await _httpClient.GetFromJsonAsync<UpdateUserDto>($"api/identity/profile/{userId}");
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<string>>($"api/identity/roles/{userId}");
        }

        public async Task<IEnumerable<LogEntryDto>> GetUserLogsAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<LogEntryDto>>($"api/identity/logs/{userId}");
        }
    }
}