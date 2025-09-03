using System.ComponentModel.DataAnnotations;

namespace Webapi.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

    }

}
