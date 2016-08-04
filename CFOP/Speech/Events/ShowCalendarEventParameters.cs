using System;

namespace CFOP.Speech.Events
{
    public class ShowCalendarEventParameters
    {
        public string Alias { get; private set; }
        public DateTime Date { get; private set; }

        public ShowCalendarEventParameters(string alias, DateTime date)
        {
            Alias = alias;
            Date = date;
        }
    }
}
