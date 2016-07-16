using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.DTO;
using Prism.Commands;
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

        public ObservableCollection<CalendarEvent> TodayEvents { get; } = new ObservableCollection<CalendarEvent>();
        private readonly IManageCalendarService _manageCalendarService;

        #endregion

        public AppointmentScheduleViewModel(IManageCalendarService manageCalendarService)
        {
            IsIdle = true;

            GetTodayScheduleCommand = DelegateCommand.FromAsyncHandler(GetTodaySchedule);

            _manageCalendarService = manageCalendarService;
        }

        #region Commands

        public ICommand GetTodayScheduleCommand { get; private set; }

        private async Task GetTodaySchedule()
        {
            IsIdle = false;
            TodayEvents.Clear();

            var events = await _manageCalendarService.FindTodayScheduleFor("david");
            TodayEvents.AddRange(events);

            IsIdle = true;
        }

        #endregion
    }
}
