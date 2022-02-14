using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class Task
    {
        [Key]
        [ForeignKey("Ticket")]
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public TaskStatus TaskStatus { get; set; }
        public Ticket Ticket { get; set; }
    }
}