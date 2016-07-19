using System;
using Prism.Commands;
using Prism.Mvvm;
using SKYPE4COMLib;

namespace CFOP.VideoCall
{
    public class VideoCallViewModel : BindableBase
    {
        private readonly Skype _skype;

        private string _skypeContact;
        public string SkypeContact
        {
            get { return _skypeContact; }
            set
            {
                _skypeContact = value;
                OnPropertyChanged(() => SkypeContact);
                SkypeVideoCallCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool _isInCall;
        public bool IsInCall
        {
            get { return _isInCall; }
            set
            {
                _isInCall = value;
                OnPropertyChanged(() => IsInCall);
                SkypeVideoCallCommand?.RaiseCanExecuteChanged();
            }
        }

        public VideoCallViewModel()
        {
            IsInCall = false;

            _skype = SetUpSkypeClient();

            SkypeVideoCallCommand = new DelegateCommand(SkypeVideoCall, 
                            () => !string.IsNullOrWhiteSpace(SkypeContact) && !IsInCall);
        }

        private Skype SetUpSkypeClient()
        {
            var skype = new Skype();
            skype.CallStatus += OnSkypeCallStatusChanged;
            skype.CallVideoStatusChanged += OnSkypeCallVideoStatusChanged;
            skype.CallVideoSendStatusChanged += OnSkypeCallVideoSendStatusChanged;
            skype.CallVideoReceiveStatusChanged += OnSkypeCallVideoReceiveStatusChanged;
            ((_ISkypeEvents_Event)skype).AttachmentStatus += OnSkypeAttachmentStatusChanged;
            return skype;
        }

        #region Commands

        public DelegateCommand SkypeVideoCallCommand { get; private set; }

        private void SkypeVideoCall()
        {
            IsInCall = true;

            AttachToSkype();

            if (!_skype.Client.IsRunning)
            {
                _skype.Client.Start();
            }

            
            _skype.PlaceCall(SkypeContact);
        }

        #endregion

        #region Skype Event Handlers

        private void OnSkypeAttachmentStatusChanged(TAttachmentStatus status)
        {
            if (status == TAttachmentStatus.apiAttachAvailable)
            {
                AttachToSkype();
            }
        }

        private void OnSkypeCallStatusChanged(Call call, TCallStatus status)
        {
            if (status == TCallStatus.clsFinished ||
                SkypeCallFailed(status))
            {
                IsInCall = false;
            }
            else if (status == TCallStatus.clsInProgress)
            {
                _skype.Client.Focus();
            }
        }

        private bool SkypeCallFailed(TCallStatus status)
        {
            return status == TCallStatus.clsFailed ||
                   status == TCallStatus.clsCancelled ||
                   status == TCallStatus.clsBusy ||
                   status == TCallStatus.clsRefused;
        }

        private void OnSkypeCallVideoStatusChanged(Call call, TCallVideoStatus status)
        {
        }

        private void OnSkypeCallVideoSendStatusChanged(Call call, TCallVideoSendStatus status)
        {
            if (status == TCallVideoSendStatus.vssAvailable)
            {
                call.StartVideoSend();
            }
        }

        private void OnSkypeCallVideoReceiveStatusChanged(Call call, TCallVideoSendStatus status)
        {
            if (status == TCallVideoSendStatus.vssAvailable)
            {
                call.StartVideoReceive();        
            }
            else if (status == TCallVideoSendStatus.vssRunning)
            {
            }
        }

        private void AttachToSkype()
        {
            _skype.Attach(5, false);
            if (_skype.CurrentUserStatus == TUserStatus.cusOffline)
            {
                _skype.ChangeUserStatus(TUserStatus.cusOnline);
            }
        }

        #endregion
    }
}
