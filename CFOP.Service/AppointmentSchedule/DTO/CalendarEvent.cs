using System;

namespace CFOP.Service.AppointmentSchedule.DTO
{
    public class CalendarEvent
    {
        public CalendarEvent(string name, DateTime startTime, DateTime endTime)
        {
            Name = name;
            StartTime = startTime;
            EndTime = endTime;
        }

        public string Name { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
    }
}
