using Microsoft.Speech.Synthesis;

namespace CFOP.Speech
{
    public static class SpeechInstance
    {
        private static SpeechSynthesizer _synthesizer;

        public static void Initialize()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }

        public static void Speak(string message)
        {
            _synthesizer.Speak(message);
        }

        public static void Dispose()
        {
            _synthesizer?.Dispose();
            _synthesizer = null;
        }
    }
}
