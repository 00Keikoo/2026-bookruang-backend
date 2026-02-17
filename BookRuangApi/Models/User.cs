using System;
using System.ComponentModel.DataAnnotations;

namespace BookRuangApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = UserRoles.User; // "Admin" or "User"

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";

        public static bool IsValid(string role)
        {
            return role == Admin || role == User;
        }
    }
}