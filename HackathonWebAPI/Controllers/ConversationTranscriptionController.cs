using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.RemoteConversation;
using Microsoft.AspNetCore.Mvc;
using HackathonWebAPI.Helpers;
using HackathonWebAPI.Models;
using Newtonsoft.Json;

namespace HackathonWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConversationTranscriptionController : ControllerBase
    {
        [HttpPost("~/UploadRecordingAsync")]
        public async Task<ActionResult> UploadRecordingAsync(IFormFile file)
        {
            string filePath;

            if (file.Length > 0)
            {
                filePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"AudioFiles\" + Path.GetRandomFileName() + Path.GetExtension(file.FileName)));

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }
                
                return Ok($"File uploaded successfully to this location: {filePath}");
            }

            return NoContent();
        }

        [HttpPost("~/StartConversationTranscriptionAsync")]
        public async Task<List<string>> StartConversationTranscriptionAsync(string wavFile)
        {
            SpeechConfig speechConfig = GetSpeechConfig();

            // Upload the audio to the service
            string conversationId = await UploadAudioStream(speechConfig, wavFile);

            // Get remote conversation transcription results
            return await DisplayConversationTranscriptionResults(speechConfig, conversationId);
        }

        private static SpeechConfig GetSpeechConfig()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("local.appsettings.json").Build();
            string speechKey = configuration.GetSection("ApiKeys").GetSection("ApiKey_Speech").Value!;
            string speechRegion = configuration.GetSection("ApiKeys").GetSection("ApiKey_Region").Value!;

            SpeechConfig speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            return speechConfig;
        }

        public static async Task<string> UploadAudioStream(SpeechConfig speechConfig, string wavFile)
        {
            AudioStreamFormat audioStreamFormat;

            speechConfig.SetProperty("ConversationTranscriptionInRoomAndOnline", "true");
            speechConfig.SetProperty("DifferentiateGuestSpeakers", "true");
            speechConfig.SetServiceProperty("transcriptionMode", "RealTimeAndAsync", ServicePropertyChannel.UriQueryParameter);
            speechConfig.SetProperty(PropertyId.Speech_LogFilename, "ConversationTranscriptionLogfilePath");

            PullAudioInputStreamCallback wavfilePullStreamCallback = BinaryAudioStreamReader.OpenWavFileStream(wavFile, out audioStreamFormat);
            AudioInputStream audioStream = AudioInputStream.CreatePullStream(wavfilePullStreamCallback, audioStreamFormat);
            string conversationId = Guid.NewGuid().ToString();

            using (var conversation = await Conversation.CreateConversationAsync(speechConfig, conversationId))
            {
                using (var conversationTranscriber = new ConversationTranscriber(AudioConfig.FromStreamInput(audioStream)))
                {
                    await conversationTranscriber.JoinConversationAsync(conversation);
                    await GetRecognizerResult(conversationTranscriber, conversationId);
                }
            }
            return conversationId;
        }

        static async Task CompleteContinuousRecognition(ConversationTranscriber recognizer)
        {
            var finishedTaskCompletionSource = new TaskCompletionSource<int>();

            recognizer.SessionStopped += (s, e) =>
            {
                finishedTaskCompletionSource.TrySetResult(0);
            };

            recognizer.Canceled += (s, e) =>
            {
                if (e.Reason == CancellationReason.Error)
                {
                    throw new ApplicationException("${e.ErrorDetails}");
                }
                finishedTaskCompletionSource.TrySetResult(0);
            };

            await recognizer.StartTranscribingAsync().ConfigureAwait(false);

            Task.WaitAny(new[] { finishedTaskCompletionSource.Task });

            await recognizer.StopTranscribingAsync().ConfigureAwait(false);
        }

        static async Task GetRecognizerResult(ConversationTranscriber recognizer, string conversationId)
        {
            string filePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"TranscriptionFiles\Transcript_" + $"{conversationId}.txt"));

            if (!System.IO.File.Exists(filePath))
                System.IO.File.CreateText(filePath).Dispose();

            recognizer.Transcribed += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    if (e.Result.Text.Length > 0)
                    {
                        System.IO.File.AppendAllText(filePath, Environment.NewLine + $"{e.Result.UserId}: {e.Result.Text}");
                    }
                }
            };
            await CompleteContinuousRecognition(recognizer);

            recognizer.Dispose();
        }

        public static async Task<List<string>> DisplayConversationTranscriptionResults(SpeechConfig speechConfig, string conversationId)
        {
            RemoteConversationTranscriptionClient client = new RemoteConversationTranscriptionClient(speechConfig);
            RemoteConversationTranscriptionOperation operation = new RemoteConversationTranscriptionOperation(conversationId, client);

            await operation.WaitForCompletionAsync(TimeSpan.FromSeconds(10), CancellationToken.None);
            var val = operation.Value.ConversationTranscriptionResults;
            List<string> results = new List<string>();

            foreach (var item in val)
            {
                results.Add($"{item.UserId}: {item.Text}");
            }

            return results;
        }

        [HttpGet("~/GetVoiceSignatureString")]
        public async Task<string> GetVoiceSignatureString(string wavFile)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("local.appsettings.json").Build();
            string speechKey = configuration.GetSection("ApiKeys").GetSection("ApiKey_Speech").Value!;
            string speechRegion = configuration.GetSection("ApiKeys").GetSection("ApiKey_Region").Value!;

            byte[] fileBytes = System.IO.File.ReadAllBytes(wavFile);
            var content = new ByteArrayContent(fileBytes);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", speechKey);
            var response = await client.PostAsync($"https://signature.{speechRegion}.cts.speech.microsoft.com/api/v1/Signature/GenerateVoiceSignatureFromByteArray", content);

            var jsonData = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<VoiceSignature>(jsonData);
          
            return JsonConvert.SerializeObject(result.Signature);
        }

        [HttpPost("~/TranscribeWithVoiceSignatureAsync")]
        public async Task<List<string>> TranscribeWithVoiceSignatureAsync(string wavFile, string voiceSignatureUser1, string voiceSignatureUser2)
        {
            SpeechConfig speechConfig = GetSpeechConfig();
            speechConfig.SetProperty("ConversationTranscriptionInRoomAndOnline", "true");
            speechConfig.SetServiceProperty("transcriptionMode", "RealTimeAndAsync", ServicePropertyChannel.UriQueryParameter);
            
            string conversationId = Guid.NewGuid().ToString();
            
            using (var conversation = await Conversation.CreateConversationAsync(speechConfig, conversationId))
            {
                using (var conversationTranscriber = new ConversationTranscriber(AudioConfig.FromWavFileInput(wavFile)))
                {
                    var speaker1 = Participant.From("Steve", "en-US", voiceSignatureUser1);
                    var speaker2 = Participant.From("Katie", "en-US", voiceSignatureUser2);
                    await conversation.AddParticipantAsync(speaker1);
                    await conversation.AddParticipantAsync(speaker2);

                    await conversationTranscriber.JoinConversationAsync(conversation);
                    await GetRecognizerResult(conversationTranscriber, conversationId);
                }
            }

            return await DisplayConversationTranscriptionResults(speechConfig, conversationId);
        }
    }
}