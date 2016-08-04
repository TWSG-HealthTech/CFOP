using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Data;
using CFOP.Common;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.DTO;
using CFOP.Speech.Events;
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

        private string _userAlias;
        public string UserAlias
        {
            get { return _userAlias; }
            set
            {
                _userAlias = value;
                OnPropertyChanged(() => UserAlias);
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
            UserAlias = "son";
            
            GetTodayScheduleCommand = DelegateCommand.FromAsyncHandler(() => GetTodaySchedule(UserAlias, DateTime.Now), () => !string.IsNullOrWhiteSpace(UserAlias));

            _eventAggregator = eventAggregator;
            _manageCalendarService = manageCalendarService;

            BindingOperations.EnableCollectionSynchronization(TodayEvents, new object());

            _subscriptionToken = _eventAggregator.SubscribeVoiceEvent<ShowCalendarEventParameters>(ShowCalendar);
        }

        private void ShowCalendar(ShowCalendarEventParameters parameters)
        {
            GetTodaySchedule(parameters.Alias, parameters.Date).ConfigureAwait(false);
        }

        #region Commands

        public DelegateCommand GetTodayScheduleCommand { get; private set; }

        private async Task GetTodaySchedule(string userAlias, DateTime date)
        {
            IsIdle = false;
            TodayEvents.Clear();

            var events = await _manageCalendarService.FindScheduleFor(userAlias, date);
            TodayEvents.AddRange(events);

            IsIdle = true;
        }

        #endregion
    }
}
