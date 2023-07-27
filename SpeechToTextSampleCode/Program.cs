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

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            string currentDirectory = Directory.GetCurrentDirectory();
            string wavFile = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\SampleFiles\katiesteve.wav"));

            speechConfig.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "2000");
            speechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "10000");

            //Conversation Transcription from audio file using Guest/Unidentified speakers
            //await ConversationTranscription.StartConversationTranscriptionAsync(speechconfig, wavFile);


            //Speech To Text from different sources
            await SpeechToTextFromDifferentSources.SpeechToTextAsync(speechConfig);
        }
    }
}