namespace CFOP.Server.Core.Calendar
{
    public class Subscription
    {
        public int Id { get; private set; }
        public int ClientId { get; private set; }
        public string CalendarId { get; private set; }

        public Subscription(string calendarId)
        {
            CalendarId = calendarId;
        }

        private Subscription() { }
    }
}
