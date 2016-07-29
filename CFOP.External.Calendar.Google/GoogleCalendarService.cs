using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.DTO;
using CFOP.Service.Common;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CFOP.External.Calendar.Google
{
    public class GoogleCalendarService : IManageCalendarService
    {
        private readonly IUserRepository _userRepository;

        //TODO: invalidate this cache when there's a change in calendar
        private static readonly Dictionary<int, Dictionary<DateTime, IList<CalendarEvent>>> _eventCache = 
            new Dictionary<int, Dictionary<DateTime, IList<CalendarEvent>>>();

        public GoogleCalendarService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IList<CalendarEvent>> FindScheduleFor(string userAlias, DateTime date)
        {
            var user = _userRepository.FindByAlias(userAlias);
            return await FindScheduleFor(user.Id, date);
        }

        public async Task<IList<CalendarEvent>> FindScheduleFor(int userId, DateTime date)
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
                            .Where(i => i.Primary.HasValue && i.Primary.Value)
                            .Select(entry => 
                                    GetCalendarEvents(service, entry, date))))
                            .SelectMany(e => e)
                            .OrderBy(e => e.StartTime)
                            .ToList();

            _eventCache[userId][date] = events;

            return events;
        }

        public async Task<bool> IsUserBusyAt(int userId, DateTime time)
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

        private UserCredential ReadUserCredential(int userId)
        {
            var user = _userRepository.FindById(userId);
            if(user == null) throw new ArgumentException($"No user with id {userId} found");

            var credPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.Personal);
            var calendarSecret = new MemoryStream(Encoding.UTF8.GetBytes(user.Calendar.Google));

            credPath = Path.Combine(credPath, ".credentials/cfop.json");

            return GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(calendarSecret).Secrets,
                new[] { CalendarService.Scope.CalendarReadonly },
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
        }
    }
}
