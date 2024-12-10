namespace DashboardApp.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CanView { get; set; }    
        public bool CanCreate { get; set; } 
        public bool CanUpdate { get; set; } 
        public bool CanDelete { get; set; }
    }
}
