using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoreUIDemo.Models
{
    public class UserModel
    {
        public long ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public string Role { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
