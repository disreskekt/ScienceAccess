using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dtos
{
    public class Register : Login
    {
        [Required(ErrorMessage = "Email not specified")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Password not specified")]
        public string Lastname { get; set; }
    }
}