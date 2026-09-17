using System.ComponentModel.DataAnnotations;

namespace CoreUIDemo.Models.ViewModels
{
    public class UserChangePasswordViewModel
    {
        public long UserId { get; set; }

        [Required]
        public string CurrentPassword { get; set; }

        [Required, StringLength(255, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "New passwrod and confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }
}