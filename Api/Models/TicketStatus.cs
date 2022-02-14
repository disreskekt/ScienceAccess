using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class TicketStatus
    {
        [Key]
        [ForeignKey("Ticket")]
        public Guid Id { get; set; }
        public string StatusName { get; set; }
        public Ticket Ticket { get; set; }
    }
}