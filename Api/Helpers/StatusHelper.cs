﻿using System;
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
            return ticket.IsCanceled == false && !ticket.IsExpired() && ticket.IsNotUsed(); //todo зачем? забыл
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

            throw new Exception("Что-то пошло не так, возможно временной статус изменился во время проверки");
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

            throw new Exception("Что-то пошло не так, возможно статус использования изменился во время проверки");
        }
        
        public static bool IsNotUsed(this Ticket ticket)
        {
            return ticket.Task is null || ticket.Task.Status == TaskStatuses.NotStarted;
        }

        public static bool IsInUse(this Ticket ticket)
        {
            return ticket.Task is not null && ticket.Task.Status == TaskStatuses.InProgress;
        }

        public static bool IsUsed(this Ticket ticket)
        {
            return ticket.Task is not null &&
                   (ticket.Task.Status == TaskStatuses.Done || ticket.Task.Status == TaskStatuses.Failed);
        }
    }
}