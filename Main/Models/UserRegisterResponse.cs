using System;
using Main.Persistence;

namespace Main.Models
{
    public class UserRegisterResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public Role Role { get; set; }
    }
}