using CFOP.Service.VideoCall;
using Prism.Commands;
using Prism.Mvvm;

namespace CFOP.VideoCall
{
    public class VideoCallViewModel : BindableBase
    {
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

        public VideoCallViewModel(IVideoService videoService)
        {
            _videoService = videoService;
            IsInCall = false;

            VideoCallCommand = new DelegateCommand(VideoCall, 
                            () => !string.IsNullOrWhiteSpace(Contact) && !IsInCall);
        }

        #region Commands

        public DelegateCommand VideoCallCommand { get; private set; }

        private void VideoCall()
        {
            //IsInCall = true;

            _videoService.Call(Contact);
        }

        #endregion
    }
}
