using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Webapi.Models
{
    public class User : IdentityUser
    {
       

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
    public class UserRole
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string RoleId { get; set; }
        public Role Role { get; set; }


    }
}
