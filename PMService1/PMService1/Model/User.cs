using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace PMService1.Model
{
    public class User
    {
        public String Email { get; set; }
        public String Name { get; set; }
        public String Password { get; set; }

        public User(string email, string name, string password)
        {
            Email = email;
            Name = name;
            Password = password;
        }

        public User()
        {
        }
    }
}