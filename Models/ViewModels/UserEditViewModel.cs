using System.ComponentModel.DataAnnotations;

namespace CoreUIDemo.Models.ViewModels
{
    public class UserEditViewModel
    {
        public long Id { get; set; }

        [Required, StringLength(50, MinimumLength = 4)]
        public string Username { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [RegularExpression("^(user|manager|admin)$", ErrorMessage = "Role must be user, manager, or admin.")]
        public string Role { get; set; }

        public bool IsActive { get; set; }
    }
}