using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dtos
{
    public class Register : Login
    {
        [Required(ErrorMessage = "Не указан Email")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Не указан пароль")]
        public string Lastname { get; set; }
    }
}