using System.Collections;
using System.Collections.Generic;
using CFOP.Service.AppointmentSchedule.DTO;

namespace CFOP.Service.AppointmentSchedule
{
    public interface IManageCalendarService
    {
        IList<CalendarEvent> FindTodayScheduleFor(string userId);
    }
}