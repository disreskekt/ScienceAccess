using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Models.Enums;

namespace Api.Models
{
    public class Task
    {
        [Key]
        [ForeignKey("Ticket")]
        public Guid Id { get; set; }
        public string DirectoryPath { get; set; } //don't forget to add "/" if you will combine with filenames
        public List<Filename> FileNames { get; set; }
        public string Comment { get; set; }
        public TaskStatuses Status { get; set; }
        public Ticket Ticket { get; set; }
    }
}