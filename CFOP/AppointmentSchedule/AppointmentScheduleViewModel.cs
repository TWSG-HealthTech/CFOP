﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CFOP.Common;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.DTO;
using CFOP.Speech;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CFOP.AppointmentSchedule
{
    public class AppointmentScheduleViewModel : BindableBase
    {
        #region Properties

        private bool _isIdle;
        public bool IsIdle
        {
            get { return _isIdle; }
            private set
            {
                _isIdle = value;
                OnPropertyChanged(() => IsIdle);
            }
        }

        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged(() => UserId);
                GetTodayScheduleCommand?.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<CalendarEvent> TodayEvents { get; } = new ObservableCollection<CalendarEvent>();
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCalendarService _manageCalendarService;
        private SubscriptionToken _subscriptionToken;

        #endregion

        public AppointmentScheduleViewModel(IEventAggregator eventAggregator, IManageCalendarService manageCalendarService)
        {
            IsIdle = true;
            UserId = "demo";

            GetTodayScheduleCommand = DelegateCommand.FromAsyncHandler(GetTodaySchedule, () => !string.IsNullOrWhiteSpace(UserId));

            _eventAggregator = eventAggregator;
            _manageCalendarService = manageCalendarService;

            BindingOperations.EnableCollectionSynchronization(TodayEvents, new object());

            var showCalendarEvent = _eventAggregator.GetEvent<ShowCalendarInvoked>();
            _subscriptionToken = showCalendarEvent.Subscribe(ShowCalendar);
        }

        private void ShowCalendar(DateTime day)
        {
            GetTodaySchedule();
        }

        #region Commands

        public DelegateCommand GetTodayScheduleCommand { get; private set; }

        private async Task GetTodaySchedule()
        {
            IsIdle = false;
            TodayEvents.Clear();

            var events = await _manageCalendarService.FindTodayScheduleFor(UserId);
            TodayEvents.AddRange(events);

            IsIdle = true;
        }

        #endregion
    }
}
