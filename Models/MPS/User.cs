using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.MPS
{
    public partial class User
    {
        public string? LoginId { get; set; } = null!;
        public string? Password { get; set; } = null!;
        public string? NewPassword { get; set; }
        public string? FirstName { get; set; } = null!;
        public string? LastName { get; set; } = null!;
        public string? MobileNumbers { get; set; }
        public string? Email { get; set; }
        public string? EmployeeId { get; set; }
        public string? Role { get; set; }
        public int? EmployeeNumber { get; set; }
        public string? EmployeeCode { get; set; }
        public string? CompanyName { get; set; }
        public bool IsActivated { get; set; }
        public bool IsModified { get; set; }
        public DateTime DateCreated { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateLastLogin { get; set; }
        public DateTime DateLastLogout { get; set; }
        public bool RememberMe { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpired { get; set; }
        public string? VerifyType { get; set; }
    }
}
