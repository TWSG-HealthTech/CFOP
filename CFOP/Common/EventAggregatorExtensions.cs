using System;
using CFOP.Speech.Events;
using Prism.Events;

namespace CFOP.Common
{
    public static class EventAggregatorExtensions
    {
        public static void Publish<TEvent, TArgs>(this IEventAggregator eventAggregator, TArgs parameters) where TEvent : PubSubEvent<TArgs>, new()
        {
            eventAggregator.GetEvent<TEvent>().Publish(parameters);
        }

        public static void PublishVoiceEvent<TArgs>(this IEventAggregator eventAggregator, TArgs parameters)
        {
            eventAggregator.GetEvent<VoiceCommandInvoked<TArgs>>().Publish(parameters);
        }

        public static SubscriptionToken Subscribe<TEvent, TArgs>(this IEventAggregator eventAggregator, Action<TArgs> handler) where TEvent : PubSubEvent<TArgs>, new()
        {
            return eventAggregator.GetEvent<TEvent>().Subscribe(handler);
        }

        public static SubscriptionToken SubscribeVoiceEvent<TArgs>(this IEventAggregator eventAggregator, Action<TArgs> handler)
        {
            return eventAggregator.GetEvent<VoiceCommandInvoked<TArgs>>().Subscribe(handler);
        }
    }
}
