using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dtos
{
    public class Login
    {
        [Required(ErrorMessage = "Не указан Email")]
        [EmailAddress(ErrorMessage = "Почта не соответствует формату")]
        public string Email { get; set; }
    
        [Required(ErrorMessage = "Не указан пароль")]
        public string Password { get; set; }
    }
}