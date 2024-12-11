using Microsoft.JSInterop;
using System;
using System.Text.Json;
using DashboardApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardApp.Services
{
    public class PermissionHelper
    {
        private readonly IJSRuntime _jsRuntime;

        public PermissionHelper(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> CheckLogin()
        {
            var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "Token");
            return !string.IsNullOrEmpty(token);
        }

        public async Task<bool> HasAccess(string action, string url)
        {
            var menusJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "UserMenus");
            if (string.IsNullOrEmpty(menusJson))
            {
                return false;
            }

            var menus = JsonSerializer.Deserialize<List<MenuSessionDto>>(menusJson);

            var menu = menus.FirstOrDefault(m => m.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
            if (menu == null) return false;

            var canAccess = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", action);
            if (canAccess == "false") return false;

            return true;
        }
    }
}
