using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class AuthorizationModel
    {
        public string Password { get; set; }
        public string Email { get; set; }

        public AuthorizationModel(string password, string email) 
        {
            Password = password;
            Email = email;
        }
    }
}
