using System.Collections.Generic;
using DashboardApp.Models;

namespace DashboardApp.Models
{
    public class MenuResponse
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<Menu> Menus { get; set; } = new List<Menu>();
    }
}
