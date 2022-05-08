using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public TicketRequest TicketRequest { get; set; }
        
        public int RoleId { get; set; }
        public Role Role { get; set; }
        
        public List<Ticket> Tickets { get; set; }
    }
}