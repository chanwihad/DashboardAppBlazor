using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using DashboardApp.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;  

namespace DashboardApp.Services
{
    public class MenuService
    {
        private readonly HttpClient _httpClient;

        public MenuService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MenuResponse> GetMenusAsync(int page, int itemsPerPage, string searchQuery = "")
        {
            return await _httpClient.GetFromJsonAsync<MenuResponse>(
                $"/api/menu?pageNumber={page}&pageSize={itemsPerPage}&searchQuery={searchQuery}");
        }



        public async Task<HttpResponseMessage> CreateMenu(Menu model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/menu", model);
            return response;
        }

        public async Task<Menu> GetMenu(int id)
        {
            
            var response = await _httpClient.GetFromJsonAsync<Menu>($"api/menu/{id}");
            return response;
        }

        public async Task<HttpResponseMessage> UpdateMenu(int id, Menu model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/menu/{id}", model);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteMenu(int id)
        {   
            var response = await _httpClient.DeleteAsync($"api/menu/{id}");
            return response;
        }
    }
}
