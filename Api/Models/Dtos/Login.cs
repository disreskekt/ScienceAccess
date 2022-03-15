using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dtos
{
    public class Login
    {
        [Required(ErrorMessage = "Email not specified")]
        [EmailAddress(ErrorMessage = "Email does not match the format")]
        public string Email { get; set; }
    
        [Required(ErrorMessage = "Password not specified")]
        public string Password { get; set; }
    }
}