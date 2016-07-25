using System.Diagnostics;
using CFOP.Service.Common;
using CFOP.Service.VideoCall;
using CFOP.Speech.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CFOP.VideoCall
{
    public class VideoCallViewModel : BindableBase
    {
        private readonly IVideoService _videoService;
        private readonly IManageUserService _manageUserService;

        private string _contact;
        public string Contact
        {
            get { return _contact; }
            set
            {
                _contact = value;
                OnPropertyChanged(() => Contact);
                VideoCallCommand?.RaiseCanExecuteChanged();
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
                VideoCallCommand?.RaiseCanExecuteChanged();
            }
        }

        public VideoCallViewModel(IEventAggregator evenAggregator, IVideoService videoService, IManageUserService manageUserService)
        {
            _videoService = videoService;
            _manageUserService = manageUserService;
            IsInCall = false;

            VideoCallCommand = new DelegateCommand(VideoCall, 
                            () => !string.IsNullOrWhiteSpace(Contact) && !IsInCall);

            evenAggregator.GetEvent<VoiceCommandInvoked<CallVideoEventParameters>>().Subscribe(VideoCall);
        }

        private void VideoCall(CallVideoEventParameters parameters)
        {
            var user = _manageUserService.LookUpUserByAlias(parameters.User);

            if (user != null)
            {
                //IsInCall = true;
                var skypeProcess = Process.Start($"skype:{user.Skype}?call&video=true");
                //_videoService.Call(Contact);
            }
        }

        #region Commands

        public DelegateCommand VideoCallCommand { get; private set; }

        private void VideoCall()
        {
            VideoCall(new CallVideoEventParameters(Contact));
        }

        #endregion
    }
}
