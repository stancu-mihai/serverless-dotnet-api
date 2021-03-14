using System;
using ServerlessDotnetApi.Persistence;

namespace ServerlessDotnetApi.Models
{
    public class UserResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public Role Role {get; set;}
    }
}