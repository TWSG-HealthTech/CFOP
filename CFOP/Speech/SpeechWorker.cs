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
            CultureInfo ci = new CultureInfo(_applicationSettings.Locale);
            _sre = new SpeechRecognitionEngine(ci);
            _sre.SetInputToDefaultAudioDevice();
            _sre.SpeechRecognized += OnSpeechRecognized;

            _sre.LoadGrammarAsync(CreateGrammar("See Fop", "Jefrey", "Brenda"));
            _sre.LoadGrammarAsync(CreateGrammar(CommonSpeechChoices.ConfirmChoices()));
            _sre.LoadGrammarAsync(CreateGrammar(CommonSpeechChoices.CancelChoices()));
            //_sre.LoadGrammarAsync(CreateTimeGrammar());

            _sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        private Grammar CreateTimeGrammar()
        {
            var setChoices = new Choices("call at", "set it to");

            var morningChoices = new GrammarBuilder(new Choices("am", "in the morning"));
            morningChoices.Append(new SemanticResultValue(true));

            var eveningChoices = new GrammarBuilder(new Choices("pm", "tonight", "at night"));
            eveningChoices.Append(new SemanticResultValue(false));

            var amOrPm = new Choices(morningChoices, eveningChoices);
            var amOrPmKey = new SemanticResultKey("AmPm", amOrPm);

            var hours = new Choices("1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "one", "two");
            var minutes = new Choices("10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "60", "o'clock", " ");

            var timeGrammarBuilder = new GrammarBuilder();
            timeGrammarBuilder.Append(setChoices);
            timeGrammarBuilder.Append(new SemanticResultKey("Hours", hours));
            timeGrammarBuilder.Append(new SemanticResultKey("Minutes", minutes));
            timeGrammarBuilder.Append(amOrPmKey);
            return new Grammar(timeGrammarBuilder);
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
//            else
//            {
//                var hours = e.Result.Semantics["Hours"].Value.ToString();
//                var minutes = e.Result.Semantics["Minutes"].Value.ToString();
//                var amPm = (bool) e.Result.Semantics["AmPm"].Value;
//            }
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
