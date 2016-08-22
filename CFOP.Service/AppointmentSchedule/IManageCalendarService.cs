using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CFOP.Service.AppointmentSchedule.Models;
using CFOP.Service.Common.Models;

namespace CFOP.Service.AppointmentSchedule
{
    public interface IManageCalendarService
    {
        Task<IList<CalendarEvent>> FindScheduleFor(string userAlias, DateTime date);
        Task<IList<CalendarEvent>> FindScheduleFor(User user, DateTime date);
        Task<bool> IsUserBusyAt(User user, DateTime time);
        Task<bool> IsUserBusyBetween(User user, DateTime from, DateTime to);
        Task CreateEventInPrimaryCalendar(User user, CalendarEvent e);
        Task<List<string>> FindPrimaryCalendarIdsFor(List<User> socialConnections);
    }
}