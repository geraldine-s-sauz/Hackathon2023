using HackathonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace HackathonWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionAnswerController : ControllerBase
    {
        //private readonly HackathonDbContext _dataContext;

        //public QuestionAnswerController(HackathonDbContext dataContext)
        //{
        //    _dataContext = dataContext;
        //}

        private static string OutputSpeechRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            switch (speechRecognitionResult.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
                    return speechRecognitionResult.Text;

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

            return "Error on OutputSpeechRecogntionResult: Speech could not be recognized";
        }

        private static async Task<string> ContinuousRecognition(SpeechRecognizer speechRecognizer)
        {
            var temp = "";
            var stopRecognition = new TaskCompletionSource<int>();

            speechRecognizer.Recognized += (s, e) =>
            {
                temp = OutputSpeechRecognitionResult(e.Result);
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
            return temp;
        }

        [HttpGet("getPromptFromAzure")]
        public async Task<ActionResult<string>> GetPromptFromAzure()
        {
            string speechKey = "e933129a85f04bba9d9342bb456d4697";
            string speechRegion = "eastasia";
            SpeechConfig speechconfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            using var speechRecognizer = new SpeechRecognizer(speechconfig, audioConfig);

            Console.WriteLine("Speak into your microphone.");
            string final = await ContinuousRecognition(speechRecognizer);

            return final;
        }

        [HttpGet("GetAzureQnA")]
        public async Task<ActionResult<string>> GetAzureQnA(string QuestionUI)
        {
            //Declarations
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.appsettings.json")
                .Build();
            string api_QnA = configuration.GetSection("ApiKeys").GetSection("ApiKey_QnA").Value!;
            Uri endpoint = new("https://eastasialanguageservice.cognitiveservices.azure.com/");
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string transcriptPath = Path.Combine(sCurrentDirectory, @"C:..\..\Hackathon2023\HackathonWebAPI\Models\SampleTranscript2.txt");
            string readTranscriptText = System.IO.File.ReadAllText(Path.GetFullPath(transcriptPath));

            //Functions
            AzureKeyCredential credential = new(api_QnA);
            QuestionAnsweringClient client = new(endpoint, credential);
            IEnumerable<TextDocument> records = new[]
            {
                new TextDocument("doc1", readTranscriptText)
            };
            AnswersFromTextOptions options = new(QuestionUI, records);
            Response<AnswersFromTextResult> response = await client.GetAnswersFromTextAsync(options);
            foreach (TextAnswer answer in response.Value.Answers)
            {
                if (answer.Confidence > .01)
                {
                    string BestAnswer = response.Value.Answers[0].Answer;
                    System.Diagnostics.Debug.WriteLine($"Q{QuestionUI}:{options.Question}");
                    System.Diagnostics.Debug.WriteLine($"A{QuestionUI}:{BestAnswer}");
                    System.Diagnostics.Debug.WriteLine($"Confidence Score: ({response.Value.Answers[0].Confidence:P2})"); //:P2 converts the result to a percentage with 2 decimals of accuracy.

                    string final = response.Value.Answers[0].ShortAnswer.Text;
                    return Ok(final);
                }
                else
                {
                    return Ok("None was mentioned during the conversation");
                }
            }
            return "Sorry. No information has been found.";
        }

        //[HttpGet("GetBingChat")]
        //public async Task<ActionResult<string>> GetBingChat(string QuestionUI, string TranscribeText)
        //{
        //    string U2 = Guid.NewGuid().ToString();
        //    string prompt = "You are a health professional whose very good at assessing patients and knows the key points of the conversation.\n"
        //            //+ "From this answer: " + Answer + ", does it answer the question: " + Question + " ?.\b"
        //            + "From this transcript: " + TranscribeText + ", What is the answer for the question: " + QuestionUI
        //            + " Please answer directly. You do not need to repeat the question.";
        //    // Construct the chat client
        //    var client = new BingChatClient(new BingChatClientOptions
        //    {
        //        // Tone used for conversation
        //        Tone = BingChatTone.Balanced,
        //        CookieU = U2,
        //        //CookieFilePath = readFile
        //    });

        //    //var message = "Do you like cats?";
        //    var answer = await client.AskAsync(prompt);

        //    Console.WriteLine($"Answer: {answer}");

        //    return Ok(answer);
        //}
    }
}