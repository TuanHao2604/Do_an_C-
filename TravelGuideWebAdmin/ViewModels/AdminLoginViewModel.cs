using System.ComponentModel.DataAnnotations;

namespace TravelGuideWebAdmin.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
