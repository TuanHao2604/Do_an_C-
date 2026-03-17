using System.ComponentModel.DataAnnotations;

namespace TravelGuideWebAdmin.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên.")]
        public string PasswordHash { get; set; }
        
        [Required]
        public string FullName { get; set; }
        
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        public string PhoneNumber { get; set; }

        public string Role { get; set; } // "Admin" or "Customer"
        
        public string Tier { get; set; } // "Normal" or "VIP"
    }
}
