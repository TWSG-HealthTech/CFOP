using System;
using System.Data.SqlClient;
using Appccelerate.StateMachine;
using CFOP.Common;
using CFOP.Speech.Events;
using Prism.Events;

namespace CFOP.AppointmentSchedule
{
    public class ScheduleConversation : IDisposable
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly PassiveStateMachine<ConversationStates, ConversationEvents> _conversation;

        public ScheduleConversation(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _conversation = InitializeConversationStateMachine();
        }

        public void Start()
        {
            _conversation.Start();
        }

        public void Fire(ConversationEvents e, object args = null)
        {
            _conversation.Fire(e, args);
        }

        public void Dispose()
        {
            _conversation.Stop();
        }

        private PassiveStateMachine<ConversationStates, ConversationEvents> InitializeConversationStateMachine()
        {
            var conversation = new PassiveStateMachine<ConversationStates, ConversationEvents>("ScheduleConversation");

            conversation.In(ConversationStates.Initial)
                .On(ConversationEvents.AskCurrentStatus)
                    .Goto(ConversationStates.EventsListed)
                .On(ConversationEvents.WantToTalk)
                    .Goto(ConversationStates.FreeTimeHighlighted);

            conversation.In(ConversationStates.EventsListed)
                .ExecuteOnEntry<DateTime>(
                    date => _eventAggregator.PublishVoiceEvent(new ShowCalendarEventParameters(date)))
                .On(ConversationEvents.WantToTalk)
                    .Goto(ConversationStates.FreeTimeHighlighted)
                .On(ConversationEvents.AskCurrentStatus)
                    .Goto(ConversationStates.EventsListed);

            conversation.In(ConversationStates.FreeTimeHighlighted)
                .On(ConversationEvents.ChooseTimeSlot)
                    .Goto(ConversationStates.Scheduled);

            conversation.Initialize(ConversationStates.Initial);

            return conversation;
        }
    }
}
