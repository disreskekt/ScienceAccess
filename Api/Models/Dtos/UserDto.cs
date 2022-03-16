using System.Collections.Generic;

namespace Api.Models.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool TicketRequest { get; set; }
        
        public string RoleName { get; set; }
        
        public List<TicketDto> Tickets { get; set; }
    }
}