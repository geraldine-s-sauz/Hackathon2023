using Microsoft.CognitiveServices.Speech;
using SpeechToTextSampleCode;

namespace QuestionAnsweringSampleCode
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string speechKey  = "e933129a85f04bba9d9342bb456d4697";
            string speechRegion = "eastasia";

            SpeechConfig speechconfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            string currentDirectory = Directory.GetCurrentDirectory();
            string wavFile = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\SampleFiles\katiesteve.wav"));

            //Conversation Transcription from audio file using Guest/Unidentified speakers
            await ConversationTranscription.StartConversationTranscriptionAsync(speechconfig, wavFile);

            //Speech To Text from different sources
            //await SpeechToTextFromDifferentSources.SpeechToTextAsync(speechconfig);
        }
    }
}