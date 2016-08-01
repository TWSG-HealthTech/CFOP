﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CFOP.Service.AppointmentSchedule.DTO;
using CFOP.Service.Common.DTO;

namespace CFOP.Service.AppointmentSchedule
{
    public interface IManageCalendarService
    {
        Task<IList<CalendarEvent>> FindScheduleFor(string userAlias, DateTime date);
        Task<IList<CalendarEvent>> FindScheduleFor(User user, DateTime date);
        Task<bool> IsUserBusyAt(User user, DateTime time);
    }
}