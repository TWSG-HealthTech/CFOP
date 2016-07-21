using System.Diagnostics;
using CFOP.Service.VideoCall;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace CFOP.VideoCall
{
    public class VideoCallViewModel : BindableBase
    {
        private readonly IEventAggregator _evenAggregator;
        private readonly IVideoService _videoService;

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

        public VideoCallViewModel(IEventAggregator evenAggregator, IVideoService videoService)
        {
            _evenAggregator = evenAggregator;
            _videoService = videoService;
            IsInCall = false;

            VideoCallCommand = new DelegateCommand(VideoCall, 
                            () => !string.IsNullOrWhiteSpace(Contact) && !IsInCall);

            _evenAggregator.GetEvent<CallVideoInvoked>().Subscribe(VideoCall);
        }

        private void VideoCall(string userId)
        {
            //TODO: map from word like "son", "daughter" to actual skype user id

            //IsInCall = true;
            var skypeProcess = Process.Start($"skype:{userId}?call&video=true");
            //_videoService.Call(Contact);
        }

        #region Commands

        public DelegateCommand VideoCallCommand { get; private set; }

        private void VideoCall()
        {
            VideoCall(Contact);
        }

        #endregion
    }
}
