using Microsoft.CognitiveServices.Speech;
using SpeechToTextSampleCode;

namespace QuestionAnsweringSampleCode
{
    class Program
    {
        private static readonly string speechKey = "e933129a85f04bba9d9342bb456d4697";
        private static readonly string speechRegion = "eastasia";

        static async Task Main(string[] args)
        {
            SpeechConfig speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            string currentDirectory = Directory.GetCurrentDirectory();
            string wavFile = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\SampleFiles\katiesteve.wav"));
            
            Console.WriteLine($"Enter Azure AI service to test:");
            Console.WriteLine("\t1 - Speech to Text from different sources");
            Console.WriteLine("\t2 - Asynchronous Conversation Transcription from audio stream file using Guest/Unidentified speakers");
            Console.WriteLine("\t3 - Real-time and Asynchronous Conversation Transcription from wav file using voice signatures");
            Console.WriteLine("\t4 - Multi-device Create Conversation");
            Console.WriteLine("\t5 - Multi-device Join Conversation");
            Console.Write("Your option? ");

            string source = Console.ReadLine();

            switch (source)
            {
                case "1":
                    //Speech To Text from different sources
                    await SpeechToTextFromDifferentSources.SpeechToTextAsync(speechConfig);
                    break;
                case "2":
                    //Asynchronous Conversation Transcription from audio stream file using Guest/Unidentified speakers
                    await ConversationTranscription.StartConversationTranscriptionAsync(speechConfig, wavFile);
                    break;
                case "3":
                    //Real-time and Asynchronous Conversation Transcription from wav file using voice signatures
                    await TranscribeFromWavFileInput(currentDirectory, speechConfig, wavFile);
                    break;
                case "4":
                    //Multi-device Create Conversation
                    await MultideviceConversation.CreateConversationAsync(speechKey, speechRegion);
                    break;
                case "5":
                    Console.Write("Enter your Conversation ID:");
                    string conversationId = Console.ReadLine();
                    //Multi-device Join Conversation
                    await MultideviceConversation.JoinConversationAsync(conversationId);
                    break;
                default:
                    break;
            }
        }

        static async Task TranscribeFromWavFileInput(string currentDirectory, SpeechConfig speechConfig, string wavFile)
        {
            string speaker1WavFile = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\SampleFiles\enrollment_audio_steve.wav"));
            byte[] speaker1FileBytes = File.ReadAllBytes(speaker1WavFile);
            string speaker1 = await ConversationTranscription.GetVoiceSignatureString(speechKey, speechRegion, speaker1FileBytes);

            string speaker2WavFile = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\SampleFiles\enrollment_audio_katie.wav"));
            byte[] speaker2FileBytes = File.ReadAllBytes(speaker2WavFile);
            string speaker2 = await ConversationTranscription.GetVoiceSignatureString(speechKey, speechRegion, speaker2FileBytes);

            await ConversationTranscription.TranscribeConversationsFromFileAsync(speechConfig, wavFile, speaker1, speaker2);
        }
    }
}