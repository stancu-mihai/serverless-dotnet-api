using System;
using Main.Persistence;

namespace Main.Models
{
    public class UserResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public Role Role {get; set;}
    }
}