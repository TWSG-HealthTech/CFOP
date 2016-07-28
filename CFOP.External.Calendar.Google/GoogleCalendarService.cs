using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.DTO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CFOP.External.Calendar.Google
{
    public class GoogleCalendarService : IManageCalendarService
    {
        //TODO: invalidate this cache when there's a change in calendar
        private static Dictionary<string, Dictionary<DateTime, IList<CalendarEvent>>> _eventCache = 
            new Dictionary<string, Dictionary<DateTime, IList<CalendarEvent>>>();

        public async Task<IList<CalendarEvent>> FindScheduleFor(string userId, DateTime date)
        {
            if (!_eventCache.ContainsKey(userId))
            {
                _eventCache[userId] = new Dictionary<DateTime, IList<CalendarEvent>>();
            }

            if (_eventCache[userId].ContainsKey(date))
            {
                return _eventCache[userId][date];
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = ReadUserCredential(userId),
                ApplicationName = "CFOP",
            });

            var calendarRequest = service.CalendarList.List();
            var calendarList = await calendarRequest.ExecuteAsync();
            var events = (await Task.WhenAll(
                calendarList.Items
                            .Select(entry => 
                                    GetCalendarEvents(service, entry, date))))
                            .SelectMany(e => e)
                            .OrderBy(e => e.StartTime)
                            .ToList();

            _eventCache[userId][date] = events;

            return events;
        }

        public async Task<bool> IsUserBusyAt(string userId, DateTime time)
        {
            var date = ExtractDateFrom(time);
            var events = await FindScheduleFor(userId, date);

            return events.Any(e => e.IsBusyAt(time));
        }

        private static DateTime ExtractDateFrom(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day);
        }

        private static async Task<IEnumerable<CalendarEvent>> GetCalendarEvents(CalendarService service, CalendarListEntry calendar, DateTime date)
        {
            var request = service.Events.List(calendar.Id);
            request.TimeMin = ExtractDateFrom(date);
            request.TimeMax = request.TimeMin.Value.AddDays(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;

            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();


            if (events.Items == null || events.Items.Count <= 0) return new List<CalendarEvent>();

            return events.Items.Select(eventItem =>
            {
                var start = ExtractTime(eventItem.Start);
                var end = ExtractTime(eventItem.End);
                return new CalendarEvent(eventItem.Summary, start, end);
            });
        }

        private static DateTime ExtractTime(EventDateTime time)
        {
            var start = time.DateTime;
            if (!start.HasValue)
            {
                start = DateTime.ParseExact(time.Date, "yyyy-MM-dd", new CultureInfo("en-US"));
            }

            return start.Value;
        }

        private static UserCredential ReadUserCredential(string userId)
        {
            var secretFilePath = $"Secrets/client_secret_{userId.ToLower()}.json";
            if(!File.Exists(secretFilePath)) throw new ArgumentException($"Google client secret file for {userId} not found");

            using (var stream = new FileStream(secretFilePath, FileMode.Open, FileAccess.Read))
            {
                var credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);

                credPath = Path.Combine(credPath, ".credentials/cfop.json");

                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new [] { CalendarService.Scope.CalendarReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
        }
    }
}
