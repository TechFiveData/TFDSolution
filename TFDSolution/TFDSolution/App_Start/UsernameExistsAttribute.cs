using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TFDSolution.Business.Interface;

namespace TFDSolution.App_Start
{
    public class UsernameExistsAttribute : ValidationAttribute
    {
        //private readonly YourDbContext _context;

        public UsernameExistsAttribute()
        {
            // Assuming you have a DbContext for accessing the database
            //_context = new YourDbContext(); // Replace with your actual DbContext
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;  // Allow empty usernames, let the required validation handle it.

            string username = Convert.ToString(value);

            // Check if the username already exists in the database
            IUserBusines userBusines = new Business.UserBusines();
            bool exists = userBusines.UserNameExist(username);
            // If username exists, return false, else return true
            return !exists;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} already exists. Please choose a different username.";
        }
    }
}