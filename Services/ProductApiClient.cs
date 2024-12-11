using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using DashboardApp.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;  
using Microsoft.JSInterop;

namespace DashboardApp.Services
{
    public class ProductApiClient
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private string _clientId;  
        private readonly string _secretKey;    
        private readonly IJSRuntime _jsRuntime;

        public ProductApiClient(HttpClient httpClient, IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
            _secretKey = _configuration["ApiSettings:SecretKey"];
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeClientIdAsync()
        {
            if (await CheckLoginAsync())
            {
                _clientId = await GetClientIdFromSession();
            }
            else
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }
        }

        private async Task<bool> CheckLoginAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "Token");
            var clientId = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "ClientId");

            return !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(clientId);
        }

        private async Task<string> GetClientIdFromSession()
        {
            return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "ClientId");
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

        private string GetTimeStamp() => DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        private void AddSecurityHeaders(string method, string rawUrl, string body)
        {
            var timeStamp = GetTimeStamp();
            var signature = GenerateSignature(method, rawUrl, _clientId, timeStamp, body);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-Client-ID", _clientId);
            _httpClient.DefaultRequestHeaders.Add("X-Time-Stamp", timeStamp);
            _httpClient.DefaultRequestHeaders.Add("X-Signature", signature);
        }

        public async Task<ProductResponse> GetProductsAsync(int page, int itemsPerPage, string searchQuery = "")
        {
            await InitializeClientIdAsync(); 
            AddSecurityHeaders("GET", "/api/products", "");
            return await _httpClient.GetFromJsonAsync<ProductResponse>(
                $"/api/products?pageNumber={page}&pageSize={itemsPerPage}&searchQuery={searchQuery}");
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            await InitializeClientIdAsync(); 
            AddSecurityHeaders("GET", $"/api/products/{id}", "");
            var response = await _httpClient.GetAsync($"/api/products/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        public async Task<HttpResponseMessage> CreateProductAsync(Product product)
        {
            await InitializeClientIdAsync(); 
            var body = JsonSerializer.Serialize(product);
            AddSecurityHeaders("POST", "/api/products", body);

            var response = await _httpClient.PostAsJsonAsync("/api/products", product);
            return response;
        }

        public async Task<HttpResponseMessage> UpdateProductAsync(int id, Product product)
        {
            await InitializeClientIdAsync(); 
            var body = JsonSerializer.Serialize(product);
            AddSecurityHeaders("PUT", $"/api/products/{id}", body);

            var response = await _httpClient.PutAsJsonAsync($"/api/products/{id}", product);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteProductAsync(int id)
        {
            await InitializeClientIdAsync(); 
            AddSecurityHeaders("DELETE", $"/api/products/{id}", "");

            var response = await _httpClient.DeleteAsync($"/api/products/{id}");
            return response;
        }
    }
}