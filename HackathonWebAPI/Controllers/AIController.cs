using HackathonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;

namespace HackathonWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenAIController : ControllerBase
    {
        private readonly HackathonDbContext _dataContext;

        public OpenAIController(HackathonDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetPrompt(string Question, string Answer)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.appsettings.json")
                .Build();

            string? temp = configuration.GetSection("ApiKeys").GetSection("ApiKey_OpenAI").Value;
            string? firstResult = null;
            //System.Diagnostics.Debug.WriteLine("APIKEY IS: " + temp);

            if (string.IsNullOrEmpty(temp))
            {
                return BadRequest();
            }
            else
            {
                OpenAIAPI openAIAPI = new(apiKeys: temp);
                string prompt = "You are a health professional whose very good at assessing patients and knows the key points of the conversation.\n"
                    //+ "From this answer: " + Answer + ", does it answer the question: " + Question + " ?.\b"
                    + "From this transcript: " + Answer + ", What is the answer for the question: " + Question
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
                else
                {
                    foreach (var completion in completions.Completions)//completions.Result.completions
                    {
                        firstResult += completion.Text;
                    }

                    System.Diagnostics.Debug.WriteLine(firstResult);

                    return Ok(firstResult);
                }
            }
        }
    }
}