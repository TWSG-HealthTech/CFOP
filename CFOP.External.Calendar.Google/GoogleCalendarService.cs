﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        public IList<CalendarEvent> FindTodayScheduleFor(string userId)
        {
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = ReadUserCredential(userId),
                ApplicationName = "CFOP",
            });
            
            var request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = request.Execute();
            var result = new List<CalendarEvent>();

            if (events.Items == null || events.Items.Count <= 0) return result;

            result.AddRange(events.Items.Select(eventItem =>
            {
                var start = ExtractTime(eventItem.Start);
                var end = ExtractTime(eventItem.End);
                return new CalendarEvent(eventItem.Summary, start, end);
            }));

            return result;
        }

        private static string ExtractTime(EventDateTime time)
        {
            var start = time.DateTime.ToString();
            if (string.IsNullOrEmpty(start))
            {
                start = time.Date;
            }

            return start;
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
