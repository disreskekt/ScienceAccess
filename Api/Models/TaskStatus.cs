using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class TaskStatus
    {
        [Key]
        [ForeignKey("Task")]
        public Guid Id { get; set; }
        public string StatusName { get; set; }
        public Task Task { get; set; }
    }
}