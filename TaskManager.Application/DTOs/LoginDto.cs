using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.DTOs
{
    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponse
    {
        public string Email { get; set; }
        public string Id { get; set; }
        public string Token { get; set; }
        public string LastLogin { get; set; }
        public List<string> Roles { get; set; }
        public string FullName { get; set; }
    }
}
