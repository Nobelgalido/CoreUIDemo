using System.ComponentModel.DataAnnotations;

namespace CoreUIDemo.Models.ViewModels
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username must be 4-50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Username may only contain letters, numbers, underscores, and periods.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [RegularExpression("^(user|manager|admin)$", ErrorMessage = "Role must be user, manager, or admin.")]
        public string Role { get; set; }
    }
}