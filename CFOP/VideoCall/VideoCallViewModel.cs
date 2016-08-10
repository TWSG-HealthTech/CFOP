using System.Windows;
using System.Windows.Threading;
using CFOP.Common;
using CFOP.Service.Common;
using CFOP.Service.VideoCall;
using CFOP.Speech.Events;
using CFOP.VideoCall.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CFOP.VideoCall
{
    public class VideoCallViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IVideoService _videoService;
        private readonly IUserRepository _userRepository;

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

        public VideoCallViewModel(IEventAggregator eventAggregator, IVideoService videoService, IUserRepository userRepository)
        {
            _eventAggregator = eventAggregator;
            _videoService = videoService;
            _userRepository = userRepository;
            IsInCall = false;

            VideoCallCommand = new DelegateCommand(VideoCall, 
                            () => !string.IsNullOrWhiteSpace(Contact) && !IsInCall);

            _eventAggregator.SubscribeVoiceEvent<CallVideoEventParameters>(VideoCall);
        }

        private void VideoCall(CallVideoEventParameters parameters)
        {
            var user = _userRepository.FindByAlias(parameters.User);

            if (user != null)
            {
                IsInCall = true;

                _eventAggregator.Publish<VideoCallStarted>();
                _videoService.Call(user, FinishVideoCall);
            }
        }

        private void FinishVideoCall()
        {
            Application.Current.Dispatcher.Invoke(() => IsInCall = false);

            _eventAggregator.Publish<VideoCallStopped>();
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
