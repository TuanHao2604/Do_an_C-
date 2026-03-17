using SQLite;

namespace TravelGuideApp.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        [Unique]
        public string Email { get; set; }
        
        [Unique]
        public string PhoneNumber { get; set; }
        
        public string Role { get; set; } // "Admin" or "Customer"
        public string Tier { get; set; } // "Normal" or "VIP"
        public int Points { get; set; }
    }
}
