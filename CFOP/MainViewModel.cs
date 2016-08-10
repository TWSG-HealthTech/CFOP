using System;
using System.Windows.Threading;
using CFOP.Infrastructure.Logging;
using CFOP.Infrastructure.Settings;
using CFOP.Speech;
using Microsoft.AspNet.SignalR.Client;
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
        private readonly ILogger unknown;

        #endregion

        public MainViewModel(SpeechWorker speechWorker, IApplicationSettings applicationSettings, ILogger logger)
        {
            IsInFullScreen = false;

            ToggleFullScreenCommand = new DelegateCommand(ToggleFullScreen);
            ExitFullScreenCommand = new DelegateCommand(ExitFullScreen);

            _speechWorker = speechWorker;
            _speechWorker.Write += WriteLine;
            _speechWorker.ShowSpeech += ShowSpeech;
            _speechWorker.Start();

//            var connection = new HubConnection(applicationSettings.ServerUrl);
//            var hub = connection.CreateHubProxy("calendarHub");
//            connection.Start().Wait();
//
//            hub.On("CalendarChanged", data => logger.Info($"Received {data} from SignalR Server"));
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