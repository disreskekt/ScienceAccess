using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Models.Enums;

namespace Api.Models
{
    public class Ticket
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AvailableDuration { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public bool IsActive
        {
            get =>
                this.IsCanceled == false &&
                this.ExpirationStatus.ToString().Equals(TicketExpirationStatuses.Pending.ToString()) ||
                this.IsCanceled == false && this.ExpirationStatus.ToString().Equals(TicketExpirationStatuses.Available.ToString()) &&
                !this.UsageStatus.ToString().Equals(TicketUsageStatuses.Used.ToString());
            private set {}
        }
        public TicketExpirationStatuses ExpirationStatus { get; set; }
        public TicketUsageStatuses UsageStatus { get; set; }
        public bool IsCanceled { get; set; }
        
        public Task Task { get; set; }
    }
}