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
        public async Task<ActionResult<OpenAI>> GetPrompt(string Question, string Answer)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string? temp = configuration.GetSection("ApiKeys").GetSection("ApiKey_OpenAI").Value;
            System.Diagnostics.Debug.WriteLine("APIKEY IS: " + temp);

            if (string.IsNullOrEmpty(temp))
            {
                return BadRequest();
            }
            else
            {
                OpenAIAPI openAIAPI = new(apiKeys: temp);
                CompletionRequest completionRequest = new()
                {
                    Prompt = "Does the answer " + "\"" + Answer + "\"" + " answer the question " + "\"" + Question + "\"" + "?. Respond by either YES or NO.",
                    Model = OpenAI_API.Models.Model.BabbageText,
                    MaxTokens = 200
                };

                var completions = await openAIAPI.Completions.CreateCompletionAsync(completionRequest);
                string? result = null;
                if (!completions.Completions.Any())//completions.Result.completions
                {
                    return BadRequest();
                }
                else
                {
                    foreach (var completion in completions.Completions)//completions.Result.completions
                    {
                        result += completion.Text;
                    }
                    System.Diagnostics.Debug.WriteLine(result);

                    if (result.Contains("yes"))
                    {
                        CompletionRequest completionRequest2 = new()
                        {
                            Prompt = "Please paraphrase this for me " + result,
                            Model = OpenAI_API.Models.Model.BabbageText,
                            MaxTokens = 200
                        };
                    }
                    return Ok(result);
                }
            }
        }
    }
}