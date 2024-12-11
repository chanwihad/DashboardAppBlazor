using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DashboardApp.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json; 
using Microsoft.JSInterop;

namespace DashboardApp.Services
{
    public class RoleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string _clientId;  
        private readonly string _secretKey;    
        private readonly IJSRuntime _jsRuntime;

        public RoleApiClient(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
            _secretKey = _configuration["ApiSettings:SecretKey"];
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeClientIdAsync()
        {
            _clientId = await GetClientIdFromSession();
        }

        private async Task<string> GetClientIdFromSession()
        {
            return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "ClientId");
        }

        public async Task<bool> IsLoggedInAsync()
        {
            var clientId = await GetClientIdFromSession();
            return !string.IsNullOrEmpty(clientId); 
        }

        public async Task<RoleResponse> GetRoles(int page, int itemsPerPage, string searchQuery = "")
        {
            if (await IsLoggedInAsync())
            {
                AddSecurityHeaders("GET", "api/role", "");
                return await _httpClient.GetFromJsonAsync<RoleResponse>(
                $"/api/role?pageNumber={page}&pageSize={itemsPerPage}&searchQuery={searchQuery}");
            }
            else
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }
        }

        public async Task<HttpResponseMessage> CreateRole(RoleRequest model)
        {
            if (await IsLoggedInAsync())
            {
                var body = JsonSerializer.Serialize(model);
                AddSecurityHeaders("POST", "api/role", body);
                var response = await _httpClient.PostAsJsonAsync("api/role", model);
                return response;
            }
            else
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }
        }

        public async Task<RoleRequest> GetRole(int id)
        {
            if (await IsLoggedInAsync())
            {
                AddSecurityHeaders("GET", $"api/role/{id}", "");
                var response = await _httpClient.GetFromJsonAsync<RoleRequest>($"api/role/{id}");
                return response;
            }
            else
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }
        }

        public async Task<HttpResponseMessage> UpdateRole(int id, RoleRequest model)
        {
            if (await IsLoggedInAsync())
            {
                var body = JsonSerializer.Serialize(model);
                AddSecurityHeaders("PUT", $"api/role/{id}", body);
                var response = await _httpClient.PutAsJsonAsync($"api/role/{id}", model);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {response.StatusCode}, Content: {errorContent}");
                }
                return response;
            }
            else
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }
        }

        public async Task<HttpResponseMessage> DeleteRole(int id)
        {
            if (await IsLoggedInAsync())
            {
                AddSecurityHeaders("DELETE", $"api/role/{id}", "");
                var response = await _httpClient.DeleteAsync($"api/role/{id}");
                return response;
            }
            else
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }
        }

        private string GenerateSignature(string method, string rawUrl, string clientId, string timeStamp, string body)
        {
            string strToSign = $"{method}:{rawUrl}:{clientId}:{timeStamp}:{body}";
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(strToSign));
                return Convert.ToBase64String(hash);
            }
        }

        private void AddSecurityHeaders(string method, string rawUrl, string body)
        {
            var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var signature = GenerateSignature(method, rawUrl, _clientId, timeStamp, body);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Client-ID", _clientId);
            _httpClient.DefaultRequestHeaders.Add("X-Time-Stamp", timeStamp);
            _httpClient.DefaultRequestHeaders.Add("X-Signature", signature);
        }
    }
}
