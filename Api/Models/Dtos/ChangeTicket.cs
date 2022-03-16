using System;

namespace Api.Models.Dtos
{
    public class ChangeTicket
    {
        public Guid Id { get; set; }
        
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? AvailableDuration { get; set; }
        
        public bool? IsCanceled { get; set; }
    }
}