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
    public class ScheduleConversation : StateMachineConversationBase<ScheduleStates, ScheduleEvents>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly SpeechSynthesizer _speechSynthesizer;

        public ScheduleConversation(IEventAggregator eventAggregator,
                                    SpeechSynthesizer speechSynthesizer)
            :base(new List<string> { "ShowCalendar" })
        {
            _eventAggregator = eventAggregator;
            _speechSynthesizer = speechSynthesizer;
        }

        public override void Handle(IntentResponse.Intent intent)
        {
            var date = intent.GetFirstIntentActionParameter("ShowCalendar", "Day");
            _speechSynthesizer.Speak($"Here is {date} schedule");

            var dateResolutionValue = intent.GetAction("ShowCalendar").GetParameter("Day").Values.First().GetResolution("date");
            var dateResolution = DateTime.ParseExact(dateResolutionValue, "yyyy-MM-dd", new CultureInfo("en-US"));

            Conversation.Fire(ScheduleEvents.AskCurrentStatus, dateResolution);
        }

        public override void HandleConfirmation()
        {
            
        }

        public override void HandleCancelling()
        {
            
        }

        protected override PassiveStateMachine<ScheduleStates, ScheduleEvents> Initialize()
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
