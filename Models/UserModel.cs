using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CoreUIDemo.Models
{
    // The attributes below carry no ErrorMessage on purpose. They are the server-side safety net;
    // every message a user reads is produced by the AngularJS form in the view. See
    // docs/superpowers/specs/2026-09-24-dataannotations-angular-validation-design.md.
    public class UserModel : IValidatableObject
    {
        public long ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        // Null on edit - the modal hides the field - and StringLength skips null values.
        // The create-only requirement is in Validate below.
        [StringLength(255, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression("^[a-zA-Z ]+$")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression("^[a-zA-Z ]+$")]
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        // Mirrors CHK_UserRole in Database/script.sql.
        [Required]
        [StringLength(50)]
        [RegularExpression("^(user|manager|admin)$")]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ID == 0 && string.IsNullOrWhiteSpace(Password))
                yield return new ValidationResult(null, new[] { "Password" });
        }
    }

    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
