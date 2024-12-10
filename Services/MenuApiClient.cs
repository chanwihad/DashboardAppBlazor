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
using System.Collections.Generic;


namespace DashboardApp.Services
{

    public class MenuApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;  
        private readonly string _secretKey;    
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public MenuApiClient(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]); 
            _secretKey = _configuration["ApiSettings:SecretKey"];
            _httpContextAccessor = httpContextAccessor;
            _clientId = _httpContextAccessor.HttpContext.Session.GetString("ClientId");
        }

        public async Task<List<Menu>> GetMenus(string searchQuery = "")
        {
            AddSecurityHeaders("GET", "api/menu", "");
            var response = await _httpClient.GetFromJsonAsync<List<Menu>>($"api/menu?searchQuery={searchQuery}");
            return response;
        }

        public async Task<HttpResponseMessage> CreateMenu(Menu model)
        {
            var body = JsonSerializer.Serialize(model);
            AddSecurityHeaders("POST", "api/menu", body);
            var response = await _httpClient.PostAsJsonAsync("api/menu", model);
            return response;
        }

        public async Task<Menu> GetMenu(int id)
        {
            AddSecurityHeaders("GET", $"api/menu/{id}", "");
            var response = await _httpClient.GetFromJsonAsync<Menu>($"api/menu/{id}");
            return response;
        }

        public async Task<HttpResponseMessage> UpdateMenu(int id, Menu model)
        {
            var body = JsonSerializer.Serialize(model);
            AddSecurityHeaders("PUT", $"api/menu/{id}", body);
            var response = await _httpClient.PutAsJsonAsync($"api/menu/{id}", model);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, Content: {errorContent}");
            }
            return response;
        }

        public async Task<HttpResponseMessage> DeleteMenu(int id)
        {
            AddSecurityHeaders("DELETE", $"api/menu/{id}", "");
            var response = await _httpClient.DeleteAsync($"api/menu/{id}");
            return response;
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