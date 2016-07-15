using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using CFOP.Infrastructure.Settings;
using CFOP.Service.AppointmentSchedule;
using CFOP.Service.AppointmentSchedule.DTO;
using Microsoft.ProjectOxford.SpeechRecognition;
using Prism.Commands;
using Prism.Mvvm;

namespace CFOP.ViewModels
{
    public class MainViewModel : BindableBase, IDisposable
    {
        #region Properties

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

        public ObservableCollection<CalendarEvent> TodayEvents { get; } = new ObservableCollection<CalendarEvent>();

        private readonly IApplicationSettings _applicationSettings;
        private readonly IManageCalendarService _manageCalendarService;
        private MicrophoneRecognitionClient _micClient;

        #endregion

        public MainViewModel(IApplicationSettings applicationSettings, IManageCalendarService manageCalendarService)
        {
            IsIdle = true;
            StartRecognitionCommand = new DelegateCommand(StartRecognition);
            GetTodayScheduleCommand = DelegateCommand.FromAsyncHandler(GetTodaySchedule);

            _applicationSettings = applicationSettings;
            _manageCalendarService = manageCalendarService;
        }

        #region Commands

        public ICommand GetTodayScheduleCommand { get; private set; }

        private async Task GetTodaySchedule()
        {
            IsIdle = false;
            TodayEvents.Clear();

            var events = await _manageCalendarService.FindTodayScheduleFor("david");
            TodayEvents.AddRange(events);

            IsIdle = true;
        }

        public ICommand StartRecognitionCommand { get; private set; }

        private void StartRecognition()
        {
            IsIdle = false;

            WriteLine("--- Start speech recognition ---");

            var subscriptionKey = _applicationSettings.SubscriptionKey;
            _micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClientWithIntent(
                "en-US",
                subscriptionKey,
                subscriptionKey,
                _applicationSettings.LuisAppId,
                _applicationSettings.LuisSubscriptionId);

            _micClient.OnIntent += OnIntentHandler;
            _micClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            _micClient.OnMicrophoneStatus += OnMicrophoneStatus;
            _micClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            _micClient.OnConversationError += OnConversationErrorHandler;

            _micClient.StartMicAndRecognition();
        }

        private void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            WriteLine("--- Intent received by OnIntentHandler() ---");
            WriteLine("{0}", e.Payload);
            WriteLine("");
        }

        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.CurrentDispatcher.Invoke((() =>
            {
                WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");
                
                _micClient.EndMicAndRecognition();

                WriteResponseResult(e);

                IsIdle = true;
            }));
        }

        private void WriteResponseResult(SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.Results.Length == 0)
            {
                WriteLine("No phrase response is available.");
            }
            else
            {
                var events = _manageCalendarService.FindTodayScheduleFor("david");
                WriteLine("********* Final n-BEST Results *********");
                for (var i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    WriteLine(
                        "[{0}] Confidence={1}, Text=\"{2}\"",
                        i,
                        e.PhraseResponse.Results[i].Confidence,
                        e.PhraseResponse.Results[i].DisplayText);
                }

                WriteLine("");
            }
        }

        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                WriteLine("--- Microphone status change received by OnMicrophoneStatus() ---");
                WriteLine("********* Microphone status: {0} *********", e.Recording);
                if (e.Recording)
                {
                    WriteLine("Please start speaking.");
                }

                IsIdle = !e.Recording;
                WriteLine("");
            });
        }

        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            this.WriteLine("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            this.WriteLine("{0}", e.PartialResult);
            this.WriteLine("");
        }

        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                IsIdle = true;
            });

            this.WriteLine("--- Error received by OnConversationErrorHandler() ---");
            this.WriteLine("Error code: {0}", e.SpeechErrorCode.ToString());
            this.WriteLine("Error text: {0}", e.SpeechErrorText);
            this.WriteLine("");
        }

        #endregion

        #region Helpers
        private void WriteLine(string format, params object[] args)
        {
            var formatted = string.Format(format, args);
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                Message += (formatted + Environment.NewLine);
            });
        }
        #endregion

        public void Dispose()
        {
            _micClient?.Dispose();
        }
    }
}