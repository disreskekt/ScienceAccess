using System;
using Api.Models;
using Api.Models.Enums;

namespace Api.Helpers
{
    public static class StatusHelper
    {
        public static bool CanBeUsedRightNow(this Ticket ticket)
        {
            return ticket.IsCanceled == false && ticket.IsAvailable() && ticket.IsNotUsed();
        }

        public static bool CanBeUsed(this Ticket ticket)
        {
            return ticket.IsCanceled == false && !ticket.IsExpired() && ticket.IsNotUsed(); //todo for what? I forgot
        }

        public static TicketExpirationStatuses GetExpirationStatus(this Ticket ticket)
        {
            if (ticket.IsPending())
            {
                return TicketExpirationStatuses.Pending;
            }

            if (ticket.IsAvailable())
            {
                return TicketExpirationStatuses.Available;
            }

            if (ticket.IsExpired())
            {
                return TicketExpirationStatuses.Expired;
            }

            throw new Exception("Something went wrong, possibly expiration status changed during checkout");
        }
        
        public static bool IsPending(this Ticket ticket)
        {
            return ticket.StartTime > DateTime.Now;
        }

        public static bool IsAvailable(this Ticket ticket)
        {
            return ticket.StartTime <= DateTime.Now && ticket.EndTime > DateTime.Now;
        }

        public static bool IsExpired(this Ticket ticket)
        {
            return ticket.EndTime <= DateTime.Now;
        }
        
        public static TicketUsageStatuses GetUsageStatus(this Ticket ticket)
        {
            if (ticket.Task is null)
            {
                throw new ArgumentNullException(nameof(ticket.Task));
            }
            
            if (ticket.IsNotUsed())
            {
                return TicketUsageStatuses.NotUsed;
            }

            if (ticket.IsInUse())
            {
                return TicketUsageStatuses.InUse;
            }

            if (ticket.IsUsed())
            {
                return TicketUsageStatuses.Used;
            }

            throw new Exception("Something went wrong, possibly usage status changed during checkout");
        }
        
        public static bool IsNotUsed(this Ticket ticket)
        {
            return ticket.Task.Status == TaskStatuses.NotStarted;
        }

        public static bool IsInUse(this Ticket ticket)
        {
            return ticket.Task.Status == TaskStatuses.InProgress || ticket.Task.Status == TaskStatuses.Pending;
        }

        public static bool IsUsed(this Ticket ticket)
        {
            return ticket.Task.Status == TaskStatuses.Done || ticket.Task.Status == TaskStatuses.Failed;
        }
    }
}