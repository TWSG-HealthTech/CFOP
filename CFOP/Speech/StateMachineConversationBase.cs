using System;
using System.Collections.Generic;
using System.Linq;
using Appccelerate.StateMachine;

namespace CFOP.Speech
{
    public abstract class StateMachineConversationBase<TStates, TEvents> : IConversation, IDisposable
        where TStates : IComparable
        where TEvents : IComparable
    {
        protected readonly IList<string> SupportedIntents;
        protected PassiveStateMachine<TStates, TEvents> Conversation;

        protected StateMachineConversationBase(IList<string> supportedIntents)
        {
            SupportedIntents = supportedIntents;
        }

        public bool CanHandle(IntentResponse.Intent intent)
        {
            return SupportedIntents.Any(i =>
                i == intent.Name && intent.IsActionTriggered(i));
        }

        public void Start()
        {
            Conversation = Initialize();

            Conversation.Start();
        }

        public abstract void Handle(IntentResponse.Intent intent);
        public abstract void HandleConfirmation();
        public abstract void HandleCancelling();
        protected abstract PassiveStateMachine<TStates, TEvents> Initialize();

        public void Dispose()
        {
            Conversation.Stop();
        }
    }
}
