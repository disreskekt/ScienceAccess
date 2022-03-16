using System;
using Api.Models.Enums;

namespace Api.Models.Dtos
{
    public class TicketDto
    {
        public Guid Id { get; set; }
        
        public int UserId { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AvailableDuration { get; set; }
        
        public bool IsCanceled { get; set; }
        
        public TicketExpirationStatuses ExpirationStatus { get; set; }
        public TicketUsageStatuses UsageStatus { get; set; }
        
        public TaskStatuses? TaskStatus { get; set; }
    }
}