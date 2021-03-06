﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Appccelerate.StateMachine;
using CFOP.Common;
using CFOP.Infrastructure.Settings;
using CFOP.Repository.Data;
using CFOP.VideoCall.Events;
using CFOPIConversation = CFOP.Speech.IConversation;
using Microsoft.ProjectOxford.SpeechRecognition;
using Microsoft.Speech.Recognition;
using Newtonsoft.Json;
using Prism.Events;

namespace CFOP.Speech
{
    public class SpeechWorker : IDisposable
    {
        private readonly IApplicationSettings _applicationSettings;
        private readonly IList<CFOPIConversation> _conversations;
        private SpeechRecognitionEngine _sre;
        private MicrophoneRecognitionClient _microphoneClient;

        public event Action<string> Write;
        public event Action<string> ShowSpeech;

        public SpeechWorker(IApplicationSettings applicationSettings,
                            IEventAggregator eventAggregator,
                            IList<CFOPIConversation> conversations)
        {
            _applicationSettings = applicationSettings;
            _conversations = conversations;

            eventAggregator.Subscribe<VideoCallStarted, object>(arg => StopLocalSpeechRecognition());
            eventAggregator.Subscribe<VideoCallStopped, object>(arg => StartLocalSpeechRecognition());
        }

        public void Start()
        {
            SpeechInstance.Initialize();

            SetupActiveListener();

            Write?.Invoke("(Speaking: I am awake)");
            SpeechInstance.Speak("I am awake");

            var ci = new CultureInfo(_applicationSettings.Locale);
            _sre = new SpeechRecognitionEngine(ci);
            _sre.SpeechRecognized += OnSpeechRecognized;

            _sre.LoadGrammarAsync(CreateGrammar("See Fop", "Jefrey", "Brenda"));
            _sre.LoadGrammarAsync(CreateGrammar(CommonSpeechChoices.ConfirmChoices()));
            _sre.LoadGrammarAsync(CreateGrammar(CommonSpeechChoices.CancelChoices()));

            _sre.SetInputToDefaultAudioDevice();
            StartLocalSpeechRecognition();
        }

        private Grammar CreateGrammar(params string[] choices)
        {
            return new Grammar(new GrammarBuilder(new Choices(choices)));
        }

        private void SetupActiveListener()
        {
            _microphoneClient?.Dispose();
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

            Write?.Invoke(string.Empty);
            Write?.Invoke($"Recognized: {txt}");
            Write?.Invoke($"Confidence: {confidence}");

            if (confidence < 0.75) return;

            if (txt.Contains("Jefrey"))
            {
                Write?.Invoke("Jefrey got woken!");
                SpeechInstance.Speak("Go away, I'm busy!");
            }
            else if (txt.Contains("Brenda"))
            {
                Write?.Invoke("Brenda got woken!");
                DoActive();
            }
            else if (txt.Contains("See Fop"))
            {
                Write?.Invoke("CFOP got woken!");
                SpeechInstance.Speak("Don't worry, you're not that old!");
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
            StopLocalSpeechRecognition();
            SetupActiveListener();
            _microphoneClient.StartMicAndRecognition();
            ShowSpeech?.Invoke("...");
        }

        private void StopLocalSpeechRecognition()
        {
            _sre.RecognizeAsyncCancel();
        }

        private void StartLocalSpeechRecognition()
        {
            _sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Write?.Invoke("--- Microphone status change received by OnMicrophoneStatus() ---");
            Write?.Invoke($"********* Microphone status: {e.Recording} *********");
            if (e.Recording)
            {
                Write?.Invoke("Listening...");
            }

            Write?.Invoke(string.Empty);
        }

        private void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            Write?.Invoke("--- Intent received by OnIntentHandler() ---");
            Write?.Invoke(e.Payload);
            Write?.Invoke(string.Empty);
            HandlePayload(e.Payload);
        }

        private void HandlePayload(string payload)
        {
            var response = JsonConvert.DeserializeObject<IntentResponse>(payload);
            var intent = response.Intents.First();
            var intentName = intent.Name;

            if (intentName == "TellJoke")
            {
                SpeechInstance.Speak("What wobbles in the sky? A jellycopter!");
            }
            else if (intentName == "ThankYou")
            {
                SpeechInstance.Speak("You're most welcome!");
            }
            else if (intentName == "ShowSocialClubs")
            {
                SpeechInstance.Speak("Please wait a moment while I look for social clubs nearby.");
                using(var context = new CateContext())
                {
                    var listOfClubs = Store.AllSocialClubs();
                    if (listOfClubs.Any())
                    {
                        listOfClubs.ForEach(sc =>
                        {
                            SpeechInstance.Speak($"The {sc.ClubName} meets at {sc.Venue}. Their contact number is {sc.ContactNumber}.");
                        }); 
                    }
                    else
                    {
                        SpeechInstance.Speak("I could not find any social clubs nearby.");
                    }
                }
            }
            else if (intentName == "ListMedication")
            {
                var medicines = Store.AllMedicines();
                if (medicines.Any())
                {
                    string list;
                    if (medicines.Count > 1)
                    {
                        list =
                            medicines.Take(medicines.Count - 1)
                                .Select(m => m.Name)
                                .Aggregate((acc, item) => $"{acc}, {item}");
                        list += $" and {medicines.Last().Name}";
                    }
                    else
                    {
                        list = medicines.Last().Name;
                    }
                    SpeechInstance.Speak($"You have {list}");
                }
                else
                {
                    SpeechInstance.Speak("You have no medicine.");
                }
            }
            else if (intentName == "BuyStuff" && intent.IsActionTriggered("BuyStuff"))
            {
                SpeechInstance.Speak($"OK, I'll add {intent.GetFirstIntentActionParameter("BuyStuff", "thing")} to your shopping!");
            }
            else if (_conversations.Any(c => c.CanHandle(intent)))
            {
                var conversation = _conversations.FirstOrDefault(c => c.CanHandle(intent));
                conversation?.Handle(intent);
            }
            else
            {
                SpeechInstance.Speak("Sorry, I don't know how to do that.");
            }
        }

        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Write?.Invoke("--- Error received by OnConversationErrorHandler() ---");
            Write?.Invoke($"Error code: {e.SpeechErrorCode}");
            Write?.Invoke($"Error text: {e.SpeechErrorText}");
            Write?.Invoke(string.Empty);
        }

        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Write?.Invoke("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            Write?.Invoke(e.PartialResult);
            Write?.Invoke(string.Empty);
            ShowSpeech?.Invoke(e.PartialResult + "...");
        }

        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            _microphoneClient.EndMicAndRecognition();
            Write?.Invoke("--- OnMicShortPhraseResponseReceivedHandler ---");
            Write?.Invoke(e.PhraseResponse.RecognitionStatus.ToString());
            if (e.PhraseResponse.Results.Length == 0)
            {
                Write?.Invoke("No phrase response is available.");
                ShowSpeech?.Invoke(string.Empty);
            }
            else
            {
                Write?.Invoke("********* Final n-BEST Results *********");

                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    Write?.Invoke(
                        $"[{i}] Confidence={e.PhraseResponse.Results[i].Confidence}, Text=\"{e.PhraseResponse.Results[i].DisplayText}\"");
                }

                Write?.Invoke(string.Empty);
                ShowSpeech?.Invoke(e.PhraseResponse.Results[0].DisplayText);
            }

            StartLocalSpeechRecognition();
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
                SpeechInstance.Dispose();
                _sre?.Dispose();
                _microphoneClient?.Dispose();
            }
            // get rid of unmanaged resources
        }
    }
}
