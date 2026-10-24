using System;
using System.Collections.Generic;

namespace DatingApp.API.Models
{
    public partial class User
    {
        public Guid Id { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Username { get; set; } = "";
        public byte[] PasswordHash { get; set; } = [];
        public byte[] PasswordSalt { get; set; } = [];
    }
}
