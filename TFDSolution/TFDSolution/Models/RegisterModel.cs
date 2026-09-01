using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TFDSolution.App_Start;


namespace TFDSolution.Models
{
    public class RegisterModel
    {

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters.")]
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        [UsernameExists(ErrorMessage = "Username is already taken. Please choose a different username.")]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords must match.")]
        [Required]
        public string ConfirmPassword { get; set; }
        [EmailAddress]
        [Required]
        public string EmailId { get; set; }

    }
}