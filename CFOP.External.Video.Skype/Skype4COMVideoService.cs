using CFOP.Service.VideoCall;
using SKYPE4COMLib;
using Common = CFOP.Service.Common.DTO;

namespace CFOP.External.Video.Skype
{
    public class Skype4COMVideoService : IVideoService
    {
        private readonly SKYPE4COMLib.Skype _skype;

        public Skype4COMVideoService()
        {
            _skype = SetUpSkypeClient();
        }

        private SKYPE4COMLib.Skype SetUpSkypeClient()
        {
            var skype = new SKYPE4COMLib.Skype();
            skype.CallStatus += OnSkypeCallStatusChanged;
            skype.CallVideoStatusChanged += OnSkypeCallVideoStatusChanged;
            skype.CallVideoSendStatusChanged += OnSkypeCallVideoSendStatusChanged;
            skype.CallVideoReceiveStatusChanged += OnSkypeCallVideoReceiveStatusChanged;
            ((_ISkypeEvents_Event)skype).AttachmentStatus += OnSkypeAttachmentStatusChanged;
            return skype;
        }

        public void Call(Common.User user)
        {
            AttachToSkype();

            if (!_skype.Client.IsRunning)
            {
                _skype.Client.Start();
            }


            _skype.PlaceCall(user.Skype);
        }

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
                //IsInCall = false;
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
