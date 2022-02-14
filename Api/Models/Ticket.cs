using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public TicketStatus TicketStatus { get; set; }
        public Task Task { get; set; }
    }
}