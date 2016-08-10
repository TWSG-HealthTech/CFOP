using System;
using System.Windows.Threading;
using CFOP.AppointmentSchedule;
using CFOP.Infrastructure.Settings;
using CFOP.Speech;
using Prism.Commands;
using Prism.Events;
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

        private string _speechText;
        public string SpeechText
        {
            get { return _speechText; }
            private set
            {
                _speechText = value;
                OnPropertyChanged(() => SpeechText);
            }
        }
        
        private readonly SpeechWorker _speechWorker;

        #endregion

        public MainViewModel(SpeechWorker speechWorker)
        {
            IsInFullScreen = false;

            ToggleFullScreenCommand = new DelegateCommand(ToggleFullScreen);
            ExitFullScreenCommand = new DelegateCommand(ExitFullScreen);

            _speechWorker = speechWorker;
            _speechWorker.Write += WriteLine;
            _speechWorker.ShowSpeech += ShowSpeech;
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

        private void ShowSpeech(string speech)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                SpeechText = speech;
            });
        }
        #endregion

        public void Dispose()
        {
            _speechWorker.Dispose();
        }
    }
}