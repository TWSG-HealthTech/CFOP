using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CFOP.Service.AppointmentSchedule.DTO;

namespace CFOP.Service.AppointmentSchedule
{
    public interface IManageCalendarService
    {
        Task<IList<CalendarEvent>> FindScheduleFor(string userId, DateTime date);
    }
}