using System.Collections.Generic;

namespace Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public bool TicketRequest { get; set; }
        
        public int RoleId { get; set; }
        public Role Role { get; set; }
        
        public List<Ticket> Tickets { get; set; }
    }
}