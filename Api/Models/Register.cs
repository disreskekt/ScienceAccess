using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Register : Login
    {
        [Required(ErrorMessage = "Не указан Email")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Не указан пароль")]
        public string SurName { get; set; }
    }
}