using System;

namespace CFOP.Speech.Events
{
    public class ShowCalendarEventParameters
    {
        public DateTime Date { get; private set; }

        public ShowCalendarEventParameters(DateTime date)
        {
            Date = date;
        }
    }
}
