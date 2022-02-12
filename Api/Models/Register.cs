using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Register : Login
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string SurName { get; set; }
    }
}