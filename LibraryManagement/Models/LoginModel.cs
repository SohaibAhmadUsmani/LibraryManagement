using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class LoginModel
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
