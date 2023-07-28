using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using Microsoft.CognitiveServices.Speech.RemoteConversation;

namespace SpeechToTextSampleCode
{
    internal class ConversationTranscription
    {
        internal static async Task<string> GetVoiceSignatureString(string subscriptionKey, string region, byte[] fileBytes)
        {
            var content = new ByteArrayContent(fileBytes);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            var response = await client.PostAsync($"https://signature.{region}.cts.speech.microsoft.com/api/v1/Signature/GenerateVoiceSignatureFromByteArray", content);

            var jsonData = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<VoiceSignature>(jsonData);
            return JsonConvert.SerializeObject(result.Signature);
        }

        public static async Task TranscribeConversationsAsync(SpeechConfig config, string filepath)
        {
            var stopRecognition = new TaskCompletionSource<int>();

            using (var audioInput = AudioConfig.FromWavFileInput(filepath))
            {
                var meetingID = Guid.NewGuid().ToString();
                using (var conversation = await Conversation.CreateConversationAsync(config, meetingID))
                {
                    // create a conversation transcriber using audio stream input
                    using (var conversationTranscriber = new ConversationTranscriber(audioInput))
                    {
                        conversationTranscriber.Transcribing += (s, e) =>
                        {
                            Console.WriteLine($"TRANSCRIBING: Text={e.Result.Text} SpeakerId={e.Result.UserId}");
                        };

                        conversationTranscriber.Transcribed += (s, e) =>
                        {
                            if (e.Result.Reason == ResultReason.RecognizedSpeech)
                            {
                                Console.WriteLine($"TRANSCRIBED: Text={e.Result.Text} SpeakerId={e.Result.UserId}");
                            }
                            else if (e.Result.Reason == ResultReason.NoMatch)
                            {
                                Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                            }
                        };

                        conversationTranscriber.Canceled += (s, e) =>
                        {
                            Console.WriteLine($"CANCELED: Reason={e.Reason}");

                            if (e.Reason == CancellationReason.Error)
                            {
                                Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                                Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                                Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                                stopRecognition.TrySetResult(0);
                            }
                        };

                        conversationTranscriber.SessionStarted += (s, e) =>
                        {
                            Console.WriteLine($"\nSession started event. SessionId={e.SessionId}");
                        };

                        conversationTranscriber.SessionStopped += (s, e) =>
                        {
                            Console.WriteLine($"\nSession stopped event. SessionId={e.SessionId}");
                            Console.WriteLine("\nStop recognition.");
                            stopRecognition.TrySetResult(0);
                        };

                        // Add participants to the conversation.
                        //var speaker1 = Participant.From("User1", "en-US", voiceSignatureStringUser1);
                        //var speaker2 = Participant.From("User2", "en-US", voiceSignatureStringUser2);
                        //await conversation.AddParticipantAsync(speaker1);
                        //await conversation.AddParticipantAsync(speaker2);

                        // Join to the conversation and start transcribing
                        await conversationTranscriber.JoinConversationAsync(conversation);
                        await conversationTranscriber.StartTranscribingAsync().ConfigureAwait(false);

                        // waits for completion, then stop transcription
                        Task.WaitAny(new[] { stopRecognition.Task });
                        await conversationTranscriber.StopTranscribingAsync().ConfigureAwait(false);
                    }
                }
            }
        }
        
        public static async Task<string> UploadAudio(SpeechConfig speechConfig, string wavFile)
        {
            AudioStreamFormat audioStreamFormat;

            speechConfig.SetProperty("ConversationTranscriptionInRoomAndOnline", "true");
            speechConfig.SetProperty("DifferentiateGuestSpeakers", "true");
            speechConfig.SetServiceProperty("transcriptionMode", "RealTimeAndAsync", ServicePropertyChannel.UriQueryParameter);

            PullAudioInputStreamCallback wavfilePullStreamCallback = BinaryAudioStreamReader.OpenWavFileStream(wavFile, out audioStreamFormat);
            AudioInputStream audioStream = AudioInputStream.CreatePullStream(wavfilePullStreamCallback, audioStreamFormat);
            string conversationId = Guid.NewGuid().ToString();

            using (var conversation = await Conversation.CreateConversationAsync(speechConfig, conversationId))
            {
                using (var conversationTranscriber = new ConversationTranscriber(AudioConfig.FromStreamInput(audioStream)))
                {
                    StartContinuousRecognition(conversationTranscriber);
                    await conversationTranscriber.JoinConversationAsync(conversation);
                    var result = await GetRecognizerResult(conversationTranscriber, conversationId);
                }
            }
            return conversationId;
        }

        public static async Task StartConversationTranscriptionAsync(SpeechConfig speechConfig, string wavFile)
        {
            // Upload the audio to the service
            string conversationId = await UploadAudio(speechConfig, wavFile);

            // Get remote conversation transcription results
            //await DisplayConversationTranscriptionResults(speechConfig, conversationId);
        }

        static ConversationTranscriber StartContinuousRecognition(ConversationTranscriber conversationTranscriber)
        {
            conversationTranscriber.SessionStarted += (s, e) =>
            {
                Console.WriteLine($"\nSession started event. SessionId={e.SessionId}");
            };
            return conversationTranscriber;
        }

        static async Task CompleteContinuousRecognition(ConversationTranscriber recognizer)
        {
            var finishedTaskCompletionSource = new TaskCompletionSource<int>();

            recognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine($"\nSession stopped event. SessionId={e.SessionId}");
                finishedTaskCompletionSource.TrySetResult(0);
            };

            recognizer.Canceled += (s, e) =>
            {
                Console.WriteLine($"CANCELED: Reason={e.Reason}");
                if (e.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                    throw new ApplicationException("${e.ErrorDetails}");
                }
                finishedTaskCompletionSource.TrySetResult(0);
            };

            await recognizer.StartTranscribingAsync().ConfigureAwait(false);

            // Waits for completion.
            // Use Task.WaitAny to keep the task rooted.
            Task.WaitAny(new[] { finishedTaskCompletionSource.Task });

            await recognizer.StopTranscribingAsync().ConfigureAwait(false);
        }

        static async Task<List<string>> GetRecognizerResult(ConversationTranscriber recognizer, string conversationId)
        {
            List<string> recognizedText = new List<string>();
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\SampleFiles\TranscriptionFiles\Transcript" + $"{conversationId}.txt"));

            if (!File.Exists(filePath))
                File.CreateText(filePath).Dispose();

            recognizer.Transcribed += (s, e) =>
            {
                if (e.Result.Text.Length > 0)
                {
                    recognizedText.Add(e.Result.Text);
                    File.AppendAllText(filePath, Environment.NewLine + $"{e.Result.UserId}: {e.Result.Text}");
                }
            };

            await CompleteContinuousRecognition(recognizer);

            recognizer.Dispose();
            return recognizedText;
        }

        public static async Task DisplayConversationTranscriptionResults(SpeechConfig speechConfig, string conversationId) 
        {
            RemoteConversationTranscriptionClient client = new RemoteConversationTranscriptionClient(speechConfig);
            RemoteConversationTranscriptionOperation operation = new RemoteConversationTranscriptionOperation(conversationId, client);

            await operation.WaitForCompletionAsync(TimeSpan.FromSeconds(10), CancellationToken.None);
            var val = operation.Value.ConversationTranscriptionResults;

            foreach (var item in val)
            {
                Console.WriteLine($"{item.UserId}: {item.Text}");
            }
            Console.WriteLine("Operation completed");
        }
    }
}