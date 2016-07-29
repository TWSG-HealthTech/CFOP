using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Appccelerate.StateMachine;
using CFOP.Common;
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
        private readonly SpeechSynthesizer _speechSynthesizer;
        private readonly PassiveStateMachine<ScheduleStates, ScheduleEvents> _conversation;

        public ScheduleConversation(IEventAggregator eventAggregator, SpeechSynthesizer speechSynthesizer)
        {
            _eventAggregator = eventAggregator;
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

        private void HandleCallVideo(IntentResponse.Intent intent)
        {
            _speechSynthesizer.Speak("Calling");
            var person = intent.GetFirstIntentActionParameter("CallVideo", "person");
            _eventAggregator.PublishVoiceEvent(new CallVideoEventParameters(person));
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
