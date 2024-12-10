using System.Collections.Generic;

namespace DashboardApp.Models
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string ClientId { get; set; }
        public List<MenuDto> Menus { get; set; }
        public PermissionDto Permissions { get; set; }
    }

    public class MenuDto
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class PermissionDto
    {
        public bool CanCreate { get; set; }
        public bool CanView { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }

}