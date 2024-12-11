using System.Collections.Generic;
using DashboardApp.Models;

namespace DashboardApp.Models
{
    public class UserResponse
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<UserRequest> Users { get; set; } = new List<UserRequest>();
    }
}
