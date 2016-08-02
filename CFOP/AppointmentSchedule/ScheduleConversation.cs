using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Appccelerate.StateMachine;
using CFOP.Common;
using CFOP.Service.AppointmentSchedule;
using CFOP.Speech;
using CFOP.Speech.Events;
using Microsoft.Speech.Synthesis;
using Prism.Events;

namespace CFOP.AppointmentSchedule
{
    public class ScheduleConversation : StateMachineConversationBase<ScheduleStates, ScheduleEvents>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCalendarService _manageCalendarService;
        private readonly SpeechSynthesizer _speechSynthesizer;
        private string _date;
        private DateTime _dateResolution;
        private string _chosenTime;
        private TimeSpan _chosenTimeResolution;

        public ScheduleConversation(IEventAggregator eventAggregator,
                                    IManageCalendarService manageCalendarService,
                                    SpeechSynthesizer speechSynthesizer)
            :base(new List<string> { "ShowCalendar", "ChooseTime" })
        {
            _eventAggregator = eventAggregator;
            _manageCalendarService = manageCalendarService;
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

                    Conversation.Fire(ScheduleEvents.ScheduleInitiated);
                    break;
                case "ChooseTime":
                    //Error handling
                    _chosenTime = intent.GetFirstIntentActionParameter("ChooseTime", "time");

                    var chosenTimeResolutionValue =
                        intent.GetAction("ChooseTime").GetParameter("time").Values.First().GetResolution("time");
                    _chosenTimeResolution = TimeSpan.FromHours(double.Parse(chosenTimeResolutionValue.Substring(1)));

                    Conversation.Fire(ScheduleEvents.TimeslotChosen);
                    break;
            }
            
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
            var conversation = new PassiveStateMachine<ScheduleStates, ScheduleEvents>("ScheduleConversation");

            conversation.In(ScheduleStates.Initial)
                .On(ScheduleEvents.ScheduleInitiated)
                .Goto(ScheduleStates.CalendarShown);

            conversation.In(ScheduleStates.CalendarShown)
                .ExecuteOnEntry(ShowCalendar)
                .On(ScheduleEvents.TimeslotChosen)
                    .If(TimeslotIsValid).Goto(ScheduleStates.WaitingConfirmation)
                    .Otherwise().Execute(PromptReselectTimeslot)
                .On(ScheduleEvents.ScheduleCancelled).Goto(ScheduleStates.Initial);

            conversation.In(ScheduleStates.WaitingConfirmation)
                .ExecuteOnEntry(PromptConfirmation)
                .On(ScheduleEvents.TimeslotChosen)
                    .If(TimeslotIsValid).Goto(ScheduleStates.WaitingConfirmation)
                    .Otherwise().Goto(ScheduleStates.CalendarShown).Execute(PromptReselectTimeslot)
                .On(ScheduleEvents.ScheduleConfirmed).Goto(ScheduleStates.Initial).Execute(CreateEventInCalendar)
                .On(ScheduleEvents.ScheduleCancelled).Goto(ScheduleStates.Initial);

            conversation.Initialize(ScheduleStates.Initial);

            return conversation;
        }

        private void ShowCalendar()
        {
            _speechSynthesizer.Speak($"Here is {_date} schedule");
            _eventAggregator.PublishVoiceEvent(new ShowCalendarEventParameters(_dateResolution));
        }

        private bool TimeslotIsValid()
        {
            //TODO: check whether selected timeslot is valid (not conflicting with calendar)
            return true;
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
            //TODO: create event in google calendar
            _speechSynthesizer.Speak("event is created in calendar");
        }
    }
}
