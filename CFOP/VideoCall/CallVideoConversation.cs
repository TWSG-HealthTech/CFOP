using System;
using System.Collections.Generic;
using Appccelerate.StateMachine;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.Common;
using CFOP.Service.Common.DTO;
using CFOP.Service.VideoCall;
using CFOP.Speech;
using Microsoft.Speech.Synthesis;

namespace CFOP.VideoCall
{
    public class CallVideoConversation : StateMachineConversationBase<CallVideoStates, CallVideoEvents>
    {
        private readonly IManageCalendarService _manageCalendarService;
        private readonly IManageUserService _manageUserService;
        private readonly IVideoService _videoService;
        private readonly SpeechSynthesizer _speechSynthesizer;

        private User _currentUser;
        private string _alias;

        public CallVideoConversation(IManageCalendarService manageCalendarService,
                                    IManageUserService manageUserService,
                                    IVideoService videoService,
                                    SpeechSynthesizer speechSynthesizer)
            :base(new List<string> { "CallVideo" })
        {
            _manageCalendarService = manageCalendarService;
            _manageUserService = manageUserService;
            _videoService = videoService;
            _speechSynthesizer = speechSynthesizer;
        }

        protected override PassiveStateMachine<CallVideoStates, CallVideoEvents> Initialize()
        {
            var conversation = new PassiveStateMachine<CallVideoStates, CallVideoEvents>();

            conversation.In(CallVideoStates.Initial)
                .ExecuteOnEntry(Reset)
                .On(CallVideoEvents.CallInitiated)
                    .If<IntentResponse.Intent>(UserIsBusy).Goto(CallVideoStates.WaitingConfirmation)
                    .Otherwise().Goto(CallVideoStates.Initial).Execute(Call);

            conversation.In(CallVideoStates.WaitingConfirmation)
                .ExecuteOnEntry(() => _speechSynthesizer.Speak($"{_alias} is busy at the moment. Do you still want to call {_alias} now?"))
                .On(CallVideoEvents.CallCancelled).Goto(CallVideoStates.Initial).Execute(() => _speechSynthesizer.Speak("OK, the call is cancelled"))
                .On(CallVideoEvents.CallConfirmed).Goto(CallVideoStates.Initial).Execute(Call);

            conversation.Initialize(CallVideoStates.Initial);

            return conversation;
        }

        public override void Handle(IntentResponse.Intent intent)
        {
            _alias = intent.GetFirstIntentActionParameter("CallVideo", "person");
            _currentUser = _manageUserService.LookUpUserByAlias(_alias);

            Conversation.Fire(CallVideoEvents.CallInitiated, intent);
        }
        public override void HandleCommonSpeech(CommonSpeechTypes type, object args)
        {
            switch (type)
            {
                case CommonSpeechTypes.Confirmation:
                    Conversation.Fire(CallVideoEvents.CallConfirmed);
                    break;
                case CommonSpeechTypes.Cancel:
                    Conversation.Fire(CallVideoEvents.CallCancelled);
                    break;
            }   
        }

        private bool UserIsBusy(IntentResponse.Intent intent)
        {
            return _manageCalendarService.IsUserBusyAt(_currentUser, DateTime.Now).Result;
        }

        private void Call()
        {
            _speechSynthesizer.Speak("Calling");
            _videoService.Call(_currentUser);
        }

        private void Reset()
        {
            _currentUser = null;
        }
    }
}
