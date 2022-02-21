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

        [NotMapped]
        public bool IsActive
        {
            get =>
                this.IsCanceled == false &&
                (this.ExpirationStatus == TicketExpirationStatuses.Pending ||
                 (this.ExpirationStatus == TicketExpirationStatuses.Available &&
                 this.UsageStatus != TicketUsageStatuses.Used));
        }
        
        [NotMapped]
        public TicketExpirationStatuses ExpirationStatus
        {
            get
            {
                if (DateTime.Now < this.StartTime)
                {
                    return TicketExpirationStatuses.Pending;
                }
                if (DateTime.Now >= this.StartTime && DateTime.Now < this.EndTime)
                {
                    return TicketExpirationStatuses.Available;
                }
                
                return TicketExpirationStatuses.Expired;
            }
        }
        
        [NotMapped]
        public TicketUsageStatuses UsageStatus
        {
            get
            {
                if (this.Task is null)
                {
                    return TicketUsageStatuses.NotUsed;
                }
                
                return this.Task.Status switch
                {
                    TaskStatuses.NotStarted => TicketUsageStatuses.NotUsed,
                    TaskStatuses.InProgress => TicketUsageStatuses.InUse,
                    _ => TicketUsageStatuses.Used
                };
            }
        }
        public bool IsCanceled { get; set; }
        
        public Task Task { get; set; }
    }
}