using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.CognitiveServices.Speech;

namespace SpeechToTextSampleCode
{
    internal class MultideviceConversation
    {
        internal static async Task CreateConversationAsync(string subscriptionKey, string region)
        {
            // Sets source and target languages.
            // Replace with the languages of your choice, from list found here: https://aka.ms/speech/sttt-languages
            string fromLanguage = "en-US";
            string toLanguage = "fil-PH";

            // Set this to the display name you want for the conversation host
            string displayName = "The host";

            // Create the task completion source that will be used to wait until the user presses Ctrl + C
            var completionSource = new TaskCompletionSource<bool>();

            // Register to listen for Ctrl + C
            Console.CancelKeyPress += (s, e) =>
            {
                completionSource.TrySetResult(true);
                e.Cancel = true; // don't terminate the current process
            };

            // Create an instance of the speech translation config
            var config = SpeechTranslationConfig.FromSubscription(subscriptionKey, region);
            config.SpeechRecognitionLanguage = fromLanguage;
            config.AddTargetLanguage(toLanguage);

            // Create the conversation
            using (var conversation = await Conversation.CreateConversationAsync(config).ConfigureAwait(false))
            {
                // Start the conversation so the host user and others can join
                await conversation.StartConversationAsync().ConfigureAwait(false);

                // Get the conversation ID. It will be up to your scenario to determine how this is shared with other participants.
                string conversationId = conversation.ConversationId;

                Console.WriteLine($"Created '{conversationId}' conversation");

                // At this point, you can use the conversation object to manage the conversation. 
                // For example, to mute everyone else in the room you can call this method:
                await conversation.MuteAllParticipantsAsync().ConfigureAwait(false);

                // Configure which audio source you want to use. If you are using a text only language, you 
                // can use the other overload of the constructor that takes no arguments
                var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                using (var conversationTranslator = new ConversationTranslator(audioConfig))
                {
                    // You should connect all the event handlers you need at this point
                    conversationTranslator.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine($"Session started: {e.SessionId}");
                    };
                    conversationTranslator.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine($"Session stopped: {e.SessionId}");
                    };
                    conversationTranslator.Canceled += (s, e) =>
                    {
                        switch (e.Reason)
                        {
                            case CancellationReason.EndOfStream:
                                Console.WriteLine($"End of audio reached");
                                break;

                            case CancellationReason.Error:
                                Console.WriteLine($"Canceled due to error. {e.ErrorCode}: {e.ErrorDetails}");
                                break;
                        }
                    };
                    conversationTranslator.ConversationExpiration += (s, e) =>
                    {
                        Console.WriteLine($"Conversation will expire in {e.ExpirationTime.TotalMinutes} minutes");
                    };

                    TranscribeConversation(conversationTranslator, conversationId, toLanguage);

                    // Enter the conversation to start receiving events
                    await conversationTranslator.JoinConversationAsync(conversation, displayName).ConfigureAwait(false);

                    // You can now send an instant message to all other participants in the room
                    await conversationTranslator.SendTextMessageAsync("The instant message to send").ConfigureAwait(false);

                    // If specified a speech to text language, you can start capturing audio
                    await conversationTranslator.StartTranscribingAsync().ConfigureAwait(false);
                    Console.WriteLine("Started transcribing. Press Ctrl + c to stop");

                    // At this point, you should start receiving transcriptions for what you are saying using the default microphone. Press Ctrl+c to stop audio capture
                    await completionSource.Task.ConfigureAwait(false);

                    // Stop audio capture
                    await conversationTranslator.StopTranscribingAsync().ConfigureAwait(false);

                    // Leave the conversation. After this you will no longer receive events
                    await conversationTranslator.LeaveConversationAsync().ConfigureAwait(false);
                }

                // End the conversation
                await conversation.EndConversationAsync().ConfigureAwait(false);

                // Delete the conversation. Any other participants that are still in the conversation will be removed
                await conversation.DeleteConversationAsync().ConfigureAwait(false);
            }
        }

        private static void TranscribeConversation(ConversationTranslator conversationTranslator, string conversationId, string language)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\SampleFiles\TranscriptionFiles\Transcript_" + $"{conversationId}.txt"));
            string translatedFilePath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\SampleFiles\TranscriptionFiles\Transcript_" + language + $"_{conversationId}.txt"));

            if (!File.Exists(filePath))
                File.CreateText(filePath).Dispose();

            var conversationParticipants = new List<ConversationParticipant>();

            conversationTranslator.ParticipantsChanged += (s, e) =>
            {
                Console.Write("The following participant(s) have ");
                switch (e.Reason)
                {
                    case ParticipantChangedReason.JoinedConversation:
                        Console.Write("joined");
                        break;

                    case ParticipantChangedReason.LeftConversation:
                        Console.Write("left");
                        break;

                    case ParticipantChangedReason.Updated:
                        Console.Write("been updated");
                        break;
                }

                Console.WriteLine(":");

                foreach (var participant in e.Participants)
                {
                    conversationParticipants.Add(new ConversationParticipant(participant.Id, participant.DisplayName));
                    Console.WriteLine($"\t{participant.DisplayName} : {participant.Id}");
                }
            };
            conversationTranslator.TextMessageReceived += (s, e) =>
            {
                Console.WriteLine($"Received an instant message from '{e.Result.ParticipantId}': '{e.Result.Text}'");
                foreach (var entry in e.Result.Translations)
                {
                    Console.WriteLine($"\tTranslated into '{entry.Key}': '{entry.Value}'");
                }
            };
            conversationTranslator.Transcribed += (s, e) =>
            {
                foreach (var participant in conversationParticipants.DistinctBy(i => i.Id))
                {
                    if (participant.Id == e.Result.ParticipantId)
                    {
                        Console.WriteLine($"TRANSCRIBED: '{participant.DisplayName}': '{e.Result.Text}'");
                        if (e.Result.Text.Length > 0)
                        {
                            File.AppendAllText(filePath, Environment.NewLine + $"{participant.DisplayName}: {e.Result.Text}");
                        }
                    }

                    foreach (var entry in e.Result.Translations)
                    {
                        if (participant.Id == e.Result.ParticipantId)
                        {
                            Console.WriteLine($"\tTranslated into '{entry.Key}': '{entry.Value}'");
                            if (entry.Value.Length > 0)
                            {
                                File.AppendAllText(translatedFilePath, Environment.NewLine + $"{participant.DisplayName}: {entry.Value}");
                            }
                        }

                    }
                }
            };
        }

        internal static async Task JoinConversationAsync(string conversationId)
        {
            // Set this to the display name you want for the participant
            string displayName = "participant";

            // Set the speech to text, or text language you want to use
            string language = "fil-PH";

            // Create the task completion source that will be used to wait until the user presses Ctrl + c
            var completionSource = new TaskCompletionSource<bool>();

            // Register to listen for Ctrl+C
            Console.CancelKeyPress += (s, e) =>
            {
                completionSource.TrySetResult(true);
                e.Cancel = true; // don't terminate the current process
            };

            // As a participant, you don't need to specify any subscription key, or region. You can directly create
            // the conversation translator object
            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using (var conversationTranslator = new ConversationTranslator(audioConfig))
            {
                TranscribeConversation(conversationTranslator, conversationId, language);

                // To start receiving events, you will need to join the conversation
                await conversationTranslator.JoinConversationAsync(conversationId, displayName, language).ConfigureAwait(false);

                // You can now send an instant message
                await conversationTranslator.SendTextMessageAsync("Message from participant").ConfigureAwait(false);

                // Start capturing audio if you specified a speech to text language
                await conversationTranslator.StartTranscribingAsync().ConfigureAwait(false);
                Console.WriteLine("Started transcribing. Press Ctrl-C to stop");

                // At this point, you should start receiving transcriptions for what you are saying using
                // the default microphone. Press Ctrl+C to stop audio capture
                await completionSource.Task.ConfigureAwait(false);

                // Stop audio capture
                await conversationTranslator.StopTranscribingAsync().ConfigureAwait(false);

                // Leave the conversation. You will stop receiving events after this
                await conversationTranslator.LeaveConversationAsync().ConfigureAwait(false);
            }
        }

        public class ConversationParticipant
        {
            public ConversationParticipant(string id, string displayName)
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException($"'{nameof(id)}' cannot be null or empty.", nameof(id));
                }

                if (string.IsNullOrEmpty(displayName))
                {
                    throw new ArgumentException($"'{nameof(displayName)}' cannot be null or empty.", nameof(displayName));
                }
                Id = id;
                DisplayName = displayName;
            }

            public string Id { get; }
            public string DisplayName { get; }
        }
    }
}