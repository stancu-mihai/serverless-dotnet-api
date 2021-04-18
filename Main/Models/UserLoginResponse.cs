using System;
using Main.Persistence;

namespace Main.Models
{
    public class UserLoginResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
    }
}