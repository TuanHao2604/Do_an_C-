using SQLite;
using System.ComponentModel.DataAnnotations;

namespace TravelGuideApp.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string FullName { get; set; }

        [Unique]
        public string Email { get; set; }

        [Unique]
        public string PhoneNumber { get; set; }

        /// <summary>"Admin" or "Customer"</summary>
        [Required]
        public string Role { get; set; }

        /// <summary>"Normal" or "VIP"</summary>
        public string Tier { get; set; } = "Normal";

        public int Points { get; set; }
    }
}
