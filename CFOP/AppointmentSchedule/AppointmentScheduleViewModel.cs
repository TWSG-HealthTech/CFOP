using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Data;
using CFOP.Common;
using CFOP.Infrastructure.Settings;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.Models;
using CFOP.Speech.Events;
using Microsoft.AspNet.SignalR.Client;
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

        private string _receivedMessage;
        public string ReceivedMessage
        {
            get { return _receivedMessage; }
            set
            {
                _receivedMessage = value;
                OnPropertyChanged(() => ReceivedMessage);
            }
        }

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnPropertyChanged(() => Connected);
            }
        }

        public ObservableCollection<CalendarEvent> TodayEvents { get; } = new ObservableCollection<CalendarEvent>();
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCalendarService _manageCalendarService;
        private readonly IApplicationSettings _applicationSettings;
        private SubscriptionToken _subscriptionToken;
        private IHubProxy _hub;
        private HubConnection _connection;

        #endregion

        public AppointmentScheduleViewModel(IEventAggregator eventAggregator, 
                                            IManageCalendarService manageCalendarService,
                                            IApplicationSettings applicationSettings)
        {
            IsIdle = true;
            UserAlias = "son";
            
            GetTodayScheduleCommand = DelegateCommand.FromAsyncHandler(() => GetTodaySchedule(UserAlias, DateTime.Now), () => !string.IsNullOrWhiteSpace(UserAlias));
            ToggleServerConnectionCommand = DelegateCommand.FromAsyncHandler(ToggleServerConnection);

            _eventAggregator = eventAggregator;
            _manageCalendarService = manageCalendarService;
            _applicationSettings = applicationSettings;

            BindingOperations.EnableCollectionSynchronization(TodayEvents, new object());

            _subscriptionToken = _eventAggregator.SubscribeVoiceEvent<ShowCalendarEventParameters>(ShowCalendar);
        }

        private void ShowCalendar(ShowCalendarEventParameters parameters)
        {
            GetTodaySchedule(parameters.Alias, parameters.Date).ConfigureAwait(false);
        }

        #region Commands

        public DelegateCommand ToggleServerConnectionCommand { get; private set; }

        private async Task ToggleServerConnection()
        {
            if (Connected)
            {
                await _hub?.Invoke("Disconnect");
                _connection.Stop();
                _hub = null;
            }
            else
            {
                _connection = new HubConnection(_applicationSettings.ServerUrl);

                /* Enable to debug SignalR connection 
                var writer = new StreamWriter("logs/server.log") {AutoFlush = true};
                _connection.TraceLevel = TraceLevels.All;
                _connection.TraceWriter = writer;
                */

                _hub = _connection.CreateHubProxy("calendarHub");
                await _connection.Start();

                _hub.On("CalendarChanged", OnCalendarChanged);
                await _hub.Invoke("Connect");
            }

            Connected = !Connected;
        }

        private void OnCalendarChanged(dynamic data)
        {
            ReceivedMessage = data;
        }

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
