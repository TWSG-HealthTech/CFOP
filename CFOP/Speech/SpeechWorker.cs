using System;
using System.Globalization;
using CFOP.Infrastructure.Settings;
using Microsoft.ProjectOxford.SpeechRecognition;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using Newtonsoft.Json;
using Prism.Events;

namespace CFOP.Speech
{
    class SpeechWorker : IDisposable
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IEventAggregator _eventAggregator;
        private readonly SpeechSynthesizer _ss = new SpeechSynthesizer();
        private SpeechRecognitionEngine _sre;
        private MicrophoneRecognitionClient _microphoneClient;

        public event Action<string> Write;

        public SpeechWorker(IApplicationSettings applicationSettings, IEventAggregator eventAggregator)
        {
            _applicationSettings = applicationSettings;
            _eventAggregator = eventAggregator;
        }

        public void Start()
        {
            SetupActiveListener();
            _ss.SetOutputToDefaultAudioDevice();
            Write("(Speaking: I am awake)");
            _ss.Speak("I am awake");
            CultureInfo ci = new CultureInfo(_applicationSettings.Locale);
            _sre = new SpeechRecognitionEngine(ci);
            _sre.SetInputToDefaultAudioDevice();
            _sre.SpeechRecognized += OnSpeechRecognized;
            Choices wakeCommands = new Choices();
            wakeCommands.Add("See Fop");
            wakeCommands.Add("Jefrey");
            wakeCommands.Add("Brenda");
            GrammarBuilder gb_Wake = new GrammarBuilder();
            gb_Wake.Append(wakeCommands);
            Grammar grammar = new Grammar(gb_Wake);
            _sre.LoadGrammarAsync(grammar);
            _sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void SetupActiveListener()
        {
            _microphoneClient = SpeechRecognitionServiceFactory.CreateMicrophoneClientWithIntent(
                _applicationSettings.Locale,
                _applicationSettings.PrimaryKey,
                _applicationSettings.SecondaryKey,
                _applicationSettings.LuisAppId,
                _applicationSettings.LuisSubscriptionId
            );

            // Event handlers for speech recognition results
            _microphoneClient.OnMicrophoneStatus += OnMicrophoneStatus;
            _microphoneClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            _microphoneClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            _microphoneClient.OnConversationError += OnConversationErrorHandler;

            // Event handler for intent result
            _microphoneClient.OnIntent += OnIntentHandler;
        }

        void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string txt = e.Result.Text;
            float confidence = e.Result.Confidence;
            Write(string.Empty);
            Write($"Recognized: {txt}");
            Write($"Confidence: {confidence}");
            if (confidence < 0.75) return;
            if (txt.Contains("Jefrey"))
            {
                Write("Jefrey got woken!");
                _ss.Speak("Go away, I'm busy!");
            }
            if (txt.Contains("Brenda"))
            {
                Write("Brenda got woken!");
                DoActive();
            }
            if (txt.Contains("See Fop"))
            {
                Write("CFOP got woken!");
                _ss.Speak("Don't worry, you're not that old!");
            }
        }

        void DoActive()
        {
            _sre.RecognizeAsyncCancel();
            _sre.RecognizeAsyncStop();

            _microphoneClient.StartMicAndRecognition();
        }

        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Write("--- Microphone status change received by OnMicrophoneStatus() ---");
            Write($"********* Microphone status: {e.Recording} *********");
            if (e.Recording)
            {
                Write("Listening...");
            }

            Write(string.Empty);
        }

        private void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            Write("--- Intent received by OnIntentHandler() ---");
            Write(e.Payload);
            Write(string.Empty);
            HandlePayload(e.Payload);
        }

        private void HandlePayload(string payload)
        {
            dynamic x = JsonConvert.DeserializeObject(payload);
            dynamic intent = x["intents"][0];
            if (intent["intent"] == "TellJoke")
            {
                _ss.Speak("What wobbles in the sky? A jellycopter!");
            }
            else if (intent["intent"] == "BuyStuff" && intent["actions"][0]["triggered"].Value)
            {
                _ss.Speak($"OK, I'll add {intent["actions"][0]["parameters"][0]["value"][0]["entity"]} to your shopping!");
            }
            else if (intent["intent"] == "ShowCalendar" && intent["actions"][0]["triggered"].Value)
            {
                _ss.Speak("Here is todays schedule");
                var day = DateTime.Today;
                _eventAggregator.GetEvent<ShowCalendarInvoked>().Publish(day);
            }
            else
            {
                _ss.Speak("Sorry, I don't know how to do that.");
            }
        }

        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Write("--- Error received by OnConversationErrorHandler() ---");
            Write($"Error code: {e.SpeechErrorCode}");
            Write($"Error text: {e.SpeechErrorText}");
            Write(string.Empty);
        }

        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Write("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            Write(e.PartialResult);
            Write(string.Empty);
        }

        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Write("--- OnMicShortPhraseResponseReceivedHandler ---");
            Write(e.PhraseResponse.RecognitionStatus.ToString());
            if (e.PhraseResponse.Results.Length == 0)
            {
                Write("No phrase response is available.");
            }
            else
            {
                Write("********* Final n-BEST Results *********");

                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    Write(
                        $"[{i}] Confidence={e.PhraseResponse.Results[i].Confidence}, Text=\"{e.PhraseResponse.Results[i].DisplayText}\"");
                }

                Write(string.Empty);
            }

            _microphoneClient.EndMicAndRecognition();

            _sre.SetInputToDefaultAudioDevice();
            _sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Dispose()
        {
            _microphoneClient.Dispose();
        }
    }
}
