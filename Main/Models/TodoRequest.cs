using System;

namespace Main.Models
{
    public class TodoRequest
    {
        public string UserEmail { get; set; }
        public string Name { get; set; }
        public int Status {get; set;}
    }
}