namespace DashboardApp.Models
{
    public class MenuModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Level1 { get; set; }
        public string? Level2 { get; set; }
        public string? Level3 { get; set; }
        public string? Level4 { get; set; }
        public string? Icon { get; set; }  
        public string Url { get; set; }  
        public bool IsSelected { get; set; }
    }
}