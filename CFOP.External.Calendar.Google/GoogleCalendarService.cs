using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CFOP.Infrastructure.Helpers;
using CFOP.Infrastructure.Settings;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.Models;
using CFOP.Service.Common;
using CFOP.Service.Common.Models;
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
        private readonly IApplicationSettings _applicationSettings;

        //TODO: invalidate this cache when there's a change in calendar
        private static readonly Dictionary<int, Dictionary<DateTime, IList<CalendarEvent>>> _eventCache = 
            new Dictionary<int, Dictionary<DateTime, IList<CalendarEvent>>>();

        public GoogleCalendarService(IUserRepository userRepository, IApplicationSettings applicationSettings)
        {
            _userRepository = userRepository;
            _applicationSettings = applicationSettings;
        }

        public async Task<IList<CalendarEvent>> FindScheduleFor(string userAlias, DateTime date)
        {
            var user = _userRepository.FindByAlias(userAlias);
            return await FindScheduleFor(user, date);
        }

        public async Task<IList<CalendarEvent>> FindScheduleFor(User user, DateTime date)
        {
            var userId = user.Id;

            if (!_eventCache.ContainsKey(userId))
            {
                _eventCache[userId] = new Dictionary<DateTime, IList<CalendarEvent>>();
            }

            if (_eventCache[userId].ContainsKey(date))
            {
                return _eventCache[userId][date];
            }

            var service = CreateCalendarService(user);

            var calendarRequest = service.CalendarList.List();
            var calendarList = await calendarRequest.ExecuteAsync();
            var preferredCalendars = user.CalendarNames.CSVToList();
            var events = (await Task.WhenAll(
                calendarList.Items
                            .Where(i => preferredCalendars.Contains(i.Summary))
                            .Select(entry => 
                                    GetCalendarEvents(service, entry, date, user))))
                            .SelectMany(e => e)
                            .OrderBy(e => e.StartTime)
                            .ToList();

            _eventCache[userId][date] = events;

            return events;
        }

        public async Task<bool> IsUserBusyAt(User user, DateTime time)
        {
            var date = time.ToDate();
            var events = await FindScheduleFor(user, date);

            return events.Any(e => e.IsBusyAt(time));
        }

        public async Task<bool> IsUserBusyBetween(User user, DateTime from, DateTime to)
        {
            var date = from.ToDate();
            var events = await FindScheduleFor(user, date);
            return events.Any(e => e.IsBusyBetween(from, to));
        }

        public async Task CreateEventInPrimaryCalendar(User user, CalendarEvent e)
        {
            var service = CreateCalendarService(user);

            var calendarRequest = service.CalendarList.List();
            var calendarList = await calendarRequest.ExecuteAsync();

            var primaryCalendar = calendarList.Items.FirstOrDefault(i =>
                i.Primary != null && i.Primary.Value);

            var insertRequest = service.Events.Insert(new Event
            {
                Summary = e.Name,
                Start = new EventDateTime { DateTime = e.StartTime },
                End = new EventDateTime { DateTime = e.EndTime },
                Attendees = new List<EventAttendee>
                {
                    new EventAttendee { Email = _applicationSettings.MainUserEmail },
                    new EventAttendee { Email = user.CalendarEmail }
                }
            },
            primaryCalendar.Id);

            await insertRequest.ExecuteAsync();

            InvalidateCache(user.Id, e.StartTime.ToDate());
        }

        public async Task<List<string>> FindPrimaryCalendarIdsFor(List<User> socialConnections)
        {
            var calendars = await Task.WhenAll(socialConnections.Select(connection =>
            {
                var service = CreateCalendarService(connection);

                var calendarRequest = service.CalendarList.Get("primary");
                return calendarRequest.ExecuteAsync();
            }));

            return calendars.Select(c => c.Id).ToList();
        }

        private void InvalidateCache(int userId, DateTime date)
        {
            if (_eventCache.ContainsKey(userId) && _eventCache[userId].ContainsKey(date))
            {
                _eventCache[userId].Remove(date);
            }
        }

        private async Task<IEnumerable<CalendarEvent>> GetCalendarEvents(
            CalendarService service, 
            CalendarListEntry calendar, 
            DateTime date, 
            User user)
        {
            var request = service.Events.List(calendar.Id);
            request.TimeMin = date.ToDate();
            request.TimeMax = request.TimeMin.Value.AddDays(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;

            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();

            if (events.Items == null || events.Items.Count <= 0) return new List<CalendarEvent>();

            return events.Items
                .Where(eventItem => eventItem.Attendees != null &&
                    eventItem.Attendees.Any(a => a.Email == user.CalendarEmail))
                .Select(eventItem =>
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

        private CalendarService CreateCalendarService(User user)
        {
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = ReadUserCredential(user),
                ApplicationName = "CFOP",
            });

            return service;
        }

        private UserCredential ReadUserCredential(User user)
        {
            if(user == null) throw new ArgumentException($"No user with id {user.Id} found");

            var calendarSecret = new MemoryStream(Encoding.UTF8.GetBytes(user.CalendarClientSecret));

            var credPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
                ".credentials/cfop.json");

            return GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(calendarSecret).Secrets,
                new[] { CalendarService.Scope.Calendar },
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
        }
    }
}
