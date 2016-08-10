using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Appccelerate.StateMachine;
using CFOP.Infrastructure.Settings;
using CFOPIConversation = CFOP.Speech.IConversation;
using Microsoft.ProjectOxford.SpeechRecognition;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using Newtonsoft.Json;

namespace CFOP.Speech
{
    public class SpeechWorker : IDisposable
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IList<CFOPIConversation> _conversations;
        private readonly SpeechSynthesizer _ss;
        private SpeechRecognitionEngine _sre;
        private MicrophoneRecognitionClient _microphoneClient;

        public event Action<string> Write;
        public event Action<string> ShowSpeech;

        public SpeechWorker(IApplicationSettings applicationSettings,
                            SpeechSynthesizer synthesizer,
                            IList<CFOPIConversation> conversations)
        {
            _applicationSettings = applicationSettings;
            _ss = synthesizer;
            _conversations = conversations;
        }

        public void Start()
        {
            SetupActiveListener();
            _ss.SetOutputToDefaultAudioDevice();
            Write("(Speaking: I am awake)");
            _ss.Speak("I am awake");

            var ci = new CultureInfo(_applicationSettings.Locale);
            _sre = new SpeechRecognitionEngine(ci);
            _sre.SetInputToDefaultAudioDevice();
            _sre.SpeechRecognized += OnSpeechRecognized;

            _sre.LoadGrammarAsync(CreateGrammar("See Fop", "Jefrey", "Brenda"));
            _sre.LoadGrammarAsync(CreateGrammar(CommonSpeechChoices.ConfirmChoices()));
            _sre.LoadGrammarAsync(CreateGrammar(CommonSpeechChoices.CancelChoices()));

            _sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        private Grammar CreateGrammar(params string[] choices)
        {
            return new Grammar(new GrammarBuilder(new Choices(choices)));
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
            var txt = e.Result.Text;
            var confidence = e.Result.Confidence;

            Write(string.Empty);
            Write($"Recognized: {txt}");
            Write($"Confidence: {confidence}");

            if (confidence < 0.75) return;

            if (txt.Contains("Jefrey"))
            {
                Write("Jefrey got woken!");
                _ss.Speak("Go away, I'm busy!");
            }
            else if (txt.Contains("Brenda"))
            {
                Write("Brenda got woken!");
                DoActive();
            }
            else if (txt.Contains("See Fop"))
            {
                Write("CFOP got woken!");
                _ss.Speak("Don't worry, you're not that old!");
            }
            else if (CommonSpeechChoices.ConfirmChoices().Any(c => c == txt))
            {
                _conversations.ForEach(c => c.HandleCommonSpeech(CommonSpeechTypes.Confirmation));
            }
            else if (CommonSpeechChoices.CancelChoices().Any(c => c == txt))
            {
                _conversations.ForEach(c => c.HandleCommonSpeech(CommonSpeechTypes.Cancel));
            }
        }

        void DoActive()
        {
            _sre.RecognizeAsyncCancel();

            _microphoneClient.StartMicAndRecognition();
            ShowSpeech("...");
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
            var response = JsonConvert.DeserializeObject<IntentResponse>(payload);
            var intent = response.Intents.First();
            var intentName = intent.Name;
            if (intentName == "TellJoke")
            {
                _ss.Speak("What wobbles in the sky? A jellycopter!");
            }
            else if (intentName == "BuyStuff" && intent.IsActionTriggered("BuyStuff"))
            {
                _ss.Speak($"OK, I'll add {intent.GetFirstIntentActionParameter("BuyStuff", "thing")} to your shopping!");
            }
            else if (_conversations.Any(c => c.CanHandle(intent)))
            {
                var conversation = _conversations.FirstOrDefault(c => c.CanHandle(intent));
                conversation?.Handle(intent);
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
            ShowSpeech(e.PartialResult + "...");

        }

        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            _microphoneClient.EndMicAndRecognition();
            Write("--- OnMicShortPhraseResponseReceivedHandler ---");
            Write(e.PhraseResponse.RecognitionStatus.ToString());
            if (e.PhraseResponse.Results.Length == 0)
            {
                Write("No phrase response is available.");
                ShowSpeech(string.Empty);
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
                ShowSpeech(e.PhraseResponse.Results[0].DisplayText);
            }

            _sre.SetInputToDefaultAudioDevice();
            _sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _ss.Dispose();
                _sre.Dispose();
                _microphoneClient.Dispose();
            }
            // get rid of unmanaged resources
        }
    }
}
