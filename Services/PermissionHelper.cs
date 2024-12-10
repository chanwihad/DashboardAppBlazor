using System;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using DashboardApp.Models;
using System.Collections.Generic;
using System.Linq;  

namespace DashboardApp.Services
{
    public class PermissionHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool CheckLogin()
        {
            var token = _httpContextAccessor.HttpContext.Session.GetString("Token");
            return !string.IsNullOrEmpty(token);
        }

        public bool HasAccess(string action, string url)
        {
            var menusJson = _httpContextAccessor.HttpContext.Session.GetString("UserMenus");
            if (string.IsNullOrEmpty(menusJson))
            {
                return false;
            }

            var menus = JsonSerializer.Deserialize<List<MenuSessionDto>>(menusJson);

            var menu = menus.FirstOrDefault(m => m.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
            if (menu == null) return false;

            var canAccess = _httpContextAccessor.HttpContext.Session.GetString(action);
            if(canAccess == "false") return false;

            return true;
        }
    }

}