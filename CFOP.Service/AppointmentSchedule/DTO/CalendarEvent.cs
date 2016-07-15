using System;

namespace CFOP.Service.AppointmentSchedule.DTO
{
    public class CalendarEvent
    {
        public CalendarEvent(string name, string startTime, string endTime)
        {
            Name = name;
            StartTime = startTime;
            EndTime = endTime;
        }

        public string Name { get; private set; }
        public string StartTime { get; private set; }
        public string EndTime { get; private set; }
    }
}
