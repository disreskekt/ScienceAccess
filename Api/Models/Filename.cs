using System;

namespace Api.Models
{
    public class Filename
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public Guid TaskId { get; set; }
        public Task Task { get; set; }
    }
}