using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace TFDSolution.Models
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public string IPAddress { get; set; }
    }

    public class ForgotModel : IValidatableObject
    {        
        public string UserName { get; set; }
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Invalid email format.")]
        public string EmailAddress { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(EmailAddress) && string.IsNullOrWhiteSpace(UserName))
            {
                yield return new ValidationResult(
                    "Either User Name or Email Address is required.",
                    new[] { nameof(EmailAddress), nameof(UserName) });
            }
        }
    }
}