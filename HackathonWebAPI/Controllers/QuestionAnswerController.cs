using HackathonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using OpenAI_API.Completions;
using OpenAI_API;

namespace HackathonWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionAnswerController : ControllerBase
    {
        private readonly HackathonDbContext _dataContext;

        public QuestionAnswerController(HackathonDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetQnA(string QuestionUI)
        {
            //Declarations
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.appsettings.json")
                .Build();
            string api_QnA = configuration.GetSection("ApiKeys").GetSection("ApiKey_QnA").Value!;
            string api_OpenAi = configuration.GetSection("ApiKeys").GetSection("ApiKey_OpenAI").Value!;
            Uri endpoint = new("https://eastasialanguageservice.cognitiveservices.azure.com/");
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string transcriptPath = Path.Combine(sCurrentDirectory, @"C:..\..\Hackathon2023\HackathonWebAPI\Models\SampleTranscript.txt");
            string readTranscriptText = System.IO.File.ReadAllText(Path.GetFullPath(transcriptPath));
            string? firstResult = null;

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
                if (answer.Confidence > .1)
                {
                    string BestAnswer = response.Value.Answers[0].Answer;
                    System.Diagnostics.Debug.WriteLine($"Q{QuestionUI}:{options.Question}");
                    System.Diagnostics.Debug.WriteLine($"A{QuestionUI}:{BestAnswer}");
                    System.Diagnostics.Debug.WriteLine($"Confidence Score: ({response.Value.Answers[0].Confidence:P2})"); //:P2 converts the result to a percentage with 2 decimals of accuracy.

                    ////////////////////////////////////////////////// PROCEED WITH OPENAI/////////////////////////////

                    OpenAIAPI openAIAPI = new(apiKeys: api_OpenAi);
                    string prompt = "You are a health professional whose very good at assessing patients and knows the key points of the conversation.\n"
                        //+ "From this answer: " + Answer + ", does it answer the question: " + Question + " ?.\b"
                        + "From this transcript: " + BestAnswer + ", What is the answer for the question: " + QuestionUI
                        + " Please answer directly. You do not need to repeat the question.";
                    CompletionRequest completionRequest = new()
                    {
                        Model = OpenAI_API.Models.Model.DavinciText,
                        MaxTokens = 200,
                        Prompt = prompt
                    };

                    var completions = await openAIAPI.Completions.CreateCompletionAsync(completionRequest);

                    if (!completions.Completions.Any())//completions.Result.completions
                    {
                        return BadRequest();
                    }

                    foreach (var completion in completions.Completions)//completions.Result.completions
                    {
                        firstResult += completion.Text;
                    }
                    System.Diagnostics.Debug.WriteLine(firstResult);
                    return Ok(firstResult);
                }
                else
                {
                    Console.WriteLine($"Q:{options.Question}");
                    Console.WriteLine("No answers met the requested confidence score.");
                    return BadRequest();
                }
            }
            return "Sorry. No information has been found.";
        }
    }
}