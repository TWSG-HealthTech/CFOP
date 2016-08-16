using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Appccelerate.StateMachine;
using CFOP.Common;
using CFOP.Infrastructure.Helpers;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.Models;
using CFOP.Service.Common;
using CFOP.Service.Common.Models;
using CFOP.Speech;
using CFOP.Speech.Events;
using Microsoft.Speech.Synthesis;
using Prism.Events;

namespace CFOP.AppointmentSchedule
{
    public class ScheduleConversation : StateMachineConversationBase<ScheduleStates, ScheduleEvents>
    {
        private const int TalkDurationInHours = 1;

        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCalendarService _manageCalendarService;
        private readonly IUserRepository _userRepository;
        private readonly SpeechSynthesizer _speechSynthesizer;

        private string _date;
        private DateTime _dateResolution;
        private string _chosenTime;
        private DateTime _chosenTimeResolution;
        private User _user;
        private string _alias;

        public ScheduleConversation(IEventAggregator eventAggregator,
                                    IManageCalendarService manageCalendarService,
                                    IUserRepository userRepository,
                                    SpeechSynthesizer speechSynthesizer)
            :base(new List<string> { "ShowCalendar", "ChooseTime" })
        {
            _eventAggregator = eventAggregator;
            _manageCalendarService = manageCalendarService;
            _userRepository = userRepository;
            _speechSynthesizer = speechSynthesizer;
        }

        public override void Handle(IntentResponse.Intent intent)
        {
            switch (intent.Name)
            {
                case "ShowCalendar":
                    _date = intent.GetFirstIntentActionParameter("ShowCalendar", "Day");

                    var dateResolutionValue = intent.GetAction("ShowCalendar").GetParameter("Day").Values.First().GetResolution("date");
                    _dateResolution = DateTime.ParseExact(dateResolutionValue, "yyyy-MM-dd", new CultureInfo("en-US"));

                    _alias = intent.GetFirstIntentActionParameter("ShowCalendar", "person");
                    _user = _userRepository.FindByAlias(_alias);

                    Conversation.Fire(ScheduleEvents.ScheduleInitiated);
                    break;
                case "ChooseTime":
                    //Error handling
                    _chosenTime = intent.GetFirstIntentActionParameter("ChooseTime", "time");

                    var chosenTimeResolutionValue =
                        intent.GetAction("ChooseTime").GetParameter("time").Values.First().GetResolution("time");
                    var parsedResolutionValue = StringToHoursAndMinutes(chosenTimeResolutionValue.Substring(TalkDurationInHours));
                    _chosenTimeResolution = 
                        DateTime.Now
                                .ToDate()
                                .AddHours(parsedResolutionValue.Item1)
                                .AddMinutes(parsedResolutionValue.Item2);

                    Conversation.Fire(ScheduleEvents.TimeslotChosen);
                    break;
            }
            
        }

        private static Tuple<int, int> StringToHoursAndMinutes(string time)
        {
            if (time.IndexOf(':') <= -1)
            {
                return Tuple.Create(int.Parse(time), 0);
            }

            var timeElements = time.Split(':');
            return Tuple.Create(int.Parse(timeElements[0]), int.Parse(timeElements[1]));
        }

        public override void HandleCommonSpeech(CommonSpeechTypes type, object args)
        {
            switch (type)
            {
                case CommonSpeechTypes.Confirmation:
                    Conversation.Fire(ScheduleEvents.ScheduleConfirmed);
                    break;
                case CommonSpeechTypes.Cancel:
                    Conversation.Fire(ScheduleEvents.ScheduleCancelled);
                    break;
            }
        }

        protected override PassiveStateMachine<ScheduleStates, ScheduleEvents> Initialize()
        {
            var conversation = new PassiveStateMachine<ScheduleStates, ScheduleEvents>();

            conversation.In(ScheduleStates.Initial)
                .On(ScheduleEvents.ScheduleInitiated)
                .Goto(ScheduleStates.CalendarShown);

            conversation.In(ScheduleStates.CalendarShown)
                .ExecuteOnEntry(ShowCalendar)
                .On(ScheduleEvents.TimeslotChosen)
                    .If(TimeslotIsValid).Goto(ScheduleStates.WaitingConfirmation)
                    .Otherwise().Execute(PromptReselectTimeslot)
                .On(ScheduleEvents.ScheduleCancelled).Goto(ScheduleStates.Initial).Execute(SayCancel);

            conversation.In(ScheduleStates.WaitingConfirmation)
                .ExecuteOnEntry(PromptConfirmation)
                .On(ScheduleEvents.TimeslotChosen)
                    .If(TimeslotIsValid).Goto(ScheduleStates.WaitingConfirmation)
                    .Otherwise().Goto(ScheduleStates.CalendarShown).Execute(PromptReselectTimeslot)
                .On(ScheduleEvents.ScheduleConfirmed).Goto(ScheduleStates.Initial).Execute(CreateEventInCalendar)
                .On(ScheduleEvents.ScheduleCancelled).Goto(ScheduleStates.Initial).Execute(SayCancel);

            conversation.Initialize(ScheduleStates.Initial);

            return conversation;
        }

        private void ShowCalendar()
        {
            _speechSynthesizer.Speak($"Here is {_date} schedule, what time do you want to schedule the talk?");
            _eventAggregator.PublishVoiceEvent(new ShowCalendarEventParameters(_alias, _dateResolution));
        }

        private bool TimeslotIsValid()
        {
            return !_manageCalendarService.IsUserBusyBetween(
                _user, 
                _chosenTimeResolution, 
                _chosenTimeResolution.AddHours(TalkDurationInHours)).Result;
        }

        private void SayCancel()
        {
            _speechSynthesizer.Speak("the scheduling is cancelled");
        }

        private void PromptConfirmation()
        {
            _speechSynthesizer.Speak("are you sure you want to call at that time?");
        }

        private void PromptReselectTimeslot()
        {
            _speechSynthesizer.Speak(
                "selected timeslot is conflicting with the schedule, please choose another timeslot");
        }

        private void CreateEventInCalendar()
        {
            _manageCalendarService.CreateEventInPrimaryCalendar(_user,
                new CalendarEvent(
                    "Talk to mom", 
                    _chosenTimeResolution, 
                    _chosenTimeResolution.AddHours(TalkDurationInHours)))
                    .Wait();

            _speechSynthesizer.Speak("event is created in calendar");
        }
    }
}
