using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace SpeechToTextSampleCode
{
    internal class SpeechToTextFromDifferentSources
    {
        static async Task ContinuousRecognition(SpeechRecognizer speechRecognizer)
        {
            var stopRecognition = new TaskCompletionSource<int>();

            speechRecognizer.Recognized += (s, e) =>
            {
                OutputSpeechRecognitionResult(e.Result);
            };

            speechRecognizer.Canceled += (s, e) =>
            {
                OutputSpeechRecognitionResult(e.Result);
                stopRecognition.TrySetResult(0);
            };

            speechRecognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine("\n    Session stopped event.");
                stopRecognition.TrySetResult(0);
            };

            await speechRecognizer.StartContinuousRecognitionAsync();

            Task.WaitAny(new[] { stopRecognition.Task });
        }

        async static Task FromFile(SpeechConfig speechConfig)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string wavFile = Path.Combine(currentDirectory, @"SampleFiles\Call1_separated_16k_health_insurance.wav");
            using var audioConfig = AudioConfig.FromWavFileInput(wavFile);
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            await ContinuousRecognition(speechRecognizer);
        }

        async static Task FromMicrophone(SpeechConfig speechConfig)
        {
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            Console.WriteLine("Speak into your microphone.");
            await ContinuousRecognition(speechRecognizer);
        }

        async static Task FromStream(SpeechConfig speechConfig)
        {
            var reader = new BinaryReader(File.OpenRead("Call1_separated_16k_health_insurance.wav"));
            using var audioConfigStream = AudioInputStream.CreatePushStream();
            using var audioConfig = AudioConfig.FromStreamInput(audioConfigStream);
            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            byte[] readBytes;
            do
            {
                readBytes = reader.ReadBytes(1024);
                audioConfigStream.Write(readBytes, readBytes.Length);
            } while (readBytes.Length > 0);

            await ContinuousRecognition(speechRecognizer);
        }

        static void OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
                    break;
                case ResultReason.NoMatch:
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(speechRecognitionResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
            }
        }

        public static async Task SpeechToTextAsync(SpeechConfig speechConfig)
        {
            Console.WriteLine($"Enter speech source:");
            Console.WriteLine("\tm - Microphone");
            Console.WriteLine("\tf - File");
            Console.WriteLine("\ts - Stream");
            Console.Write("Your option? ");

            string source = Console.ReadLine();

            switch (source)
            {
                case "m":
                    await FromMicrophone(speechConfig);
                    break;
                case "f":
                    await FromFile(speechConfig);
                    break;
                case "s":
                    await FromStream(speechConfig);
                    break;
                default:
                    break;
            }
        }
    }
}