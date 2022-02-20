using System;
using Api.Models.Enums;

namespace Api.Models.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public bool TicketRequest { get; set; }
        public Guid? ActiveTicket { get; set; }
        public TaskStatuses? TaskStatus { get; set; }
    }
}