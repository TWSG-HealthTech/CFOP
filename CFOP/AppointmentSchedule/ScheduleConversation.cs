using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Appccelerate.StateMachine;
using CFOP.Common;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.Common;
using CFOP.Service.VideoCall;
using CFOP.Speech;
using CFOP.Speech.Events;
using Microsoft.Speech.Synthesis;
using Prism.Events;

namespace CFOP.AppointmentSchedule
{
    public class ScheduleConversation : IDisposable
    {
        private readonly IList<string> _supportedIntents = new List<string> { "ShowCalendar", "CallVideo" };
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCalendarService _manageCalendarService;
        private readonly IManageUserService _manageUserService;
        private readonly IVideoService _videoService;
        private readonly SpeechSynthesizer _speechSynthesizer;
        private readonly PassiveStateMachine<ScheduleStates, ScheduleEvents> _conversation;

        public ScheduleConversation(IEventAggregator eventAggregator, 
                                    IManageCalendarService manageCalendarService,
                                    IManageUserService manageUserService, 
                                    IVideoService videoService,
                                    SpeechSynthesizer speechSynthesizer)
        {
            _eventAggregator = eventAggregator;
            _manageCalendarService = manageCalendarService;
            _manageUserService = manageUserService;
            _videoService = videoService;
            _speechSynthesizer = speechSynthesizer;
            _conversation = InitializeConversationStateMachine();
        }

        public bool CanHandle(IntentResponse.Intent intent)
        {
            return _supportedIntents.Any(i => 
                i == intent.Name && intent.IsActionTriggered(i));
        }

        public void Handle(IntentResponse.Intent intent)
        {
            switch (intent.Name)
            {
                case "ShowCalendar":
                    HandleShowCalendar(intent);
                    break;
                case "CallVideo":
                    HandleCallVideo(intent);
                    break;
                default:
                    throw new ArgumentException($"Not supported intent {intent.Name}");
            }
        }

        public void Start()
        {
            _conversation.Start();
        }

        public void Dispose()
        {
            _conversation.Stop();
        }

        private void Fire(ScheduleEvents e, object args = null)
        {
            _conversation.Fire(e, args);
        }

        private void HandleShowCalendar(IntentResponse.Intent intent)
        {
            var date = intent.GetFirstIntentActionParameter("ShowCalendar", "Day");
            _speechSynthesizer.Speak($"Here is {date} schedule");

            var dateResolutionValue = intent.GetAction("ShowCalendar").GetParameter("Day").Values.First().GetResolution("date");
            var dateResolution = DateTime.ParseExact(dateResolutionValue, "yyyy-MM-dd", new CultureInfo("en-US"));
            Fire(ScheduleEvents.AskCurrentStatus, dateResolution);
        }

        private async Task HandleCallVideo(IntentResponse.Intent intent)
        {
            var person = intent.GetFirstIntentActionParameter("CallVideo", "person");
            var user = _manageUserService.LookUpUserByAlias(person);

            if (await _manageCalendarService.IsUserBusyAt(user, DateTime.Now))
            {
                _speechSynthesizer.Speak($"{person} is busy at the moment. Do you still want to call {person} now?");
            }
            else
            {
                _speechSynthesizer.Speak("Calling");
                _videoService.Call(user);
            }
        }

        private PassiveStateMachine<ScheduleStates, ScheduleEvents> InitializeConversationStateMachine()
        {
            var conversation = new PassiveStateMachine<ScheduleStates, ScheduleEvents>("ScheduleConversation");

            conversation.In(ScheduleStates.Initial)
                .On(ScheduleEvents.AskCurrentStatus)
                    .Goto(ScheduleStates.EventsListed)
                .On(ScheduleEvents.WantToTalk)
                    .Goto(ScheduleStates.FreeTimeHighlighted);

            conversation.In(ScheduleStates.EventsListed)
                .ExecuteOnEntry<DateTime>(
                    date => _eventAggregator.PublishVoiceEvent(new ShowCalendarEventParameters(date)))
                .On(ScheduleEvents.WantToTalk)
                    .Goto(ScheduleStates.FreeTimeHighlighted)
                .On(ScheduleEvents.AskCurrentStatus)
                    .Goto(ScheduleStates.EventsListed);

            conversation.In(ScheduleStates.FreeTimeHighlighted)
                .On(ScheduleEvents.ChooseTimeSlot)
                    .Goto(ScheduleStates.Scheduled);

            conversation.Initialize(ScheduleStates.Initial);

            return conversation;
        }
    }
}
