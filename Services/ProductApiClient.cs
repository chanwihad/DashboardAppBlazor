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
using System.Collections.Generic;

namespace DashboardApp.Services
{
    public class ProductApiClient
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _clientId;  
        private readonly string _secretKey;    
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public ProductApiClient(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]); 
            _secretKey = _configuration["ApiSettings:SecretKey"];
            _httpContextAccessor = httpContextAccessor;
            _clientId = _httpContextAccessor.HttpContext.Session.GetString("ClientId");
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

        public async Task<List<Product>> GetProductsAsync()
        {
            AddSecurityHeaders("GET", "/api/products", "");
            var response = await _httpClient.GetAsync("/api/products");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Product>>();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            AddSecurityHeaders("GET", $"/api/products/{id}", "");
            var response = await _httpClient.GetAsync($"/api/products/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            var body = JsonSerializer.Serialize(product);
            AddSecurityHeaders("POST", "/api/products", body);

            var response = await _httpClient.PostAsJsonAsync("/api/products", product);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        public async Task UpdateProductAsync(int id, Product product)
        {
            var body = JsonSerializer.Serialize(product);
            AddSecurityHeaders("PUT", $"/api/products/{id}", body);

            var response = await _httpClient.PutAsJsonAsync($"/api/products/{id}", product);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteProductAsync(int id)
        {
            AddSecurityHeaders("DELETE", $"/api/products/{id}", "");

            var response = await _httpClient.DeleteAsync($"/api/products/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}