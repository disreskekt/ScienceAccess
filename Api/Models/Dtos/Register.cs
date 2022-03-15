using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dtos
{
    public class Register : Login
    {
        [Required(ErrorMessage = "Name not specified")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Lastname not specified")]
        public string Lastname { get; set; }
    }
}