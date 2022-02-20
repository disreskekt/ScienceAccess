using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dtos
{
    public class Login
    {
        [Required(ErrorMessage = "Не указан Email")]
        [EmailAddress]
        public string Email { get; set; }
    
        [Required(ErrorMessage = "Не указан пароль")]
        public string Password { get; set; }
    }
}