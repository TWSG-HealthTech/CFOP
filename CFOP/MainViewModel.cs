using System;
using System.Windows.Threading;
using CFOP.Infrastructure.Settings;
using CFOP.Speech;
using Microsoft.ProjectOxford.SpeechRecognition;
using Prism.Commands;
using Prism.Mvvm;
using ICommand = System.Windows.Input.ICommand;

namespace CFOP
{
    public class MainViewModel : BindableBase, IDisposable
    {
        #region Properties

        private bool _isInFullScreen;
        public bool IsInFullScreen
        {
            get { return _isInFullScreen; }
            set
            {
                _isInFullScreen = value;
                OnPropertyChanged(() => IsInFullScreen);
            }
        }

        private bool _isIdle;
        public bool IsIdle
        {
            get { return _isIdle; }
            private set
            {
                _isIdle = value;
                OnPropertyChanged(() => IsIdle);
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            private set
            {
                _message = value;
                OnPropertyChanged(() => Message);
            }
        }               

        private readonly IApplicationSettings _applicationSettings;        
        private MicrophoneRecognitionClient _micClient;
        private SpeechWorker _speechWorker;

        #endregion

        public MainViewModel(IApplicationSettings applicationSettings)
        {
            IsInFullScreen = false;
            IsIdle = true;

            ToggleFullScreenCommand = new DelegateCommand(ToggleFullScreen);
            ExitFullScreenCommand = new DelegateCommand(ExitFullScreen);

            _applicationSettings = applicationSettings;
            _speechWorker = new SpeechWorker();
            _speechWorker.Write += WriteLine;
            _speechWorker.Start();
        }



        #region Commands    

        public ICommand ExitFullScreenCommand { get; private set; }

        private void ExitFullScreen()
        {
            if (IsInFullScreen)
            {
                ToggleFullScreen();    
            }            
        }

        public ICommand ToggleFullScreenCommand { get; private set; }

        private void ToggleFullScreen()
        {
            IsInFullScreen = !IsInFullScreen;
        }

        #endregion

        #region Helpers
        private void WriteLine(string text)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Message += (text + Environment.NewLine);
            });
        }
        #endregion

        public void Dispose()
        {
            _micClient?.Dispose();
        }
    }
}