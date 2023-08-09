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
        //private readonly HackathonDbContext _dataContext;

        //public OpenAIController(HackathonDbContext dataContext)
        //{
        //    _dataContext = dataContext;
        //}

        // GET: api/YourController/GetPrompt
        [HttpGet("GetPrompt")]
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
                    MaxTokens = 500,
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
            
        // GET: api/YourController/GetPromptFromTranscript
        [HttpGet("GetPromptFromTranscript")]
        public async Task<ActionResult<string>> GetPromptFromTranscript(string Question)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.appsettings.json")
                .Build();

            string? temp = configuration.GetSection("ApiKeys").GetSection("ApiKey_OpenAI").Value;
            string? firstResult = null;


            string tempTranscript = "INT. COMMUNITY ASSESSMENT - DAY\r\n\r\nHi, good day. I am your health coordinator, John Bautista.\r\n\r\nHow are you today?\r\n\r\nNot so good, I am feeling a bit under the weather today. And also, just stressed out because of a big presentation coming up.\r\n\r\nI'm sorry to hear that but don't worry, I'm here to assess you and see how we can help.\r\n\r\nGreat, thank you\r\n\r\nOkay, first, may I know the full name of the patient?\r\n\r\nMy name is Alex Santos.\r\n\r\nAlright. Can you tell me what medical conditions you are having right now?\r\n\r\nSure. I have hypertension and arthritis. Also, I'm having migraines today, could be due to lack of sleep. I usually just manage them with rest and water but it is worse this day for some reason.\r\n\r\nI'm sorry to hear that. This is noted.\r\n\r\nDo you have allergies?\r\n\r\nI don't have any allergies.\r\n\r\nBy any chance are you taking any medication for these illness?\r\n\r\nYes, I am taking Amlodipine for my blood pressure and Ibuprofen for when my joints act up.\r\n\r\nAlright, Can you tell me about your vaccine history?\r\n\r\nYes. I have just gotten my third Pfizer Covid vaccine a week ago. My left arm felt a little heavy but other than that, I did not feel any side effects.\r\n\r\nHow about your first and second vaccine?\r\n\r\nI first got it last June 2022 and then the second one on January 2023. Luckily I did not feel any side effects either.\r\n\r\nThat's good to hear. So that is all for our assessment, any additional information that you might have missed?\r\n\r\nNot that I think of, but if anything else comes up I 'll let you know.\r\n\r\nAlright then, I think we are all set here. Thank you so much for your time. Have a great day.\r\n\r\nThank you too. Have a great day.\r\n\r\nIn summary, the patient is suffering from migraine due to stress and lack of sleep. She's taking Amlodipine for hypertension and has history of arthritis. She completed her 3rd doze of covid vaccine a week ago with no side effects.";
            if (string.IsNullOrEmpty(temp))
            {
                return BadRequest();
            }
            else
            {
                OpenAIAPI openAIAPI = new(apiKeys: temp);
                string prompt = "You are a health professional whose very good at assessing patients and knows the key points of the conversation.\n"
                    //+ "From this answer: " + Answer + ", does it answer the question: " + Question + " ?.\b"
                    + "From this transcript: " + tempTranscript + ", What is the answer for the question: " + Question
                    + " Please answer directly. You do not need to repeat the question.";
                CompletionRequest completionRequest = new()
                {
                    Model = OpenAI_API.Models.Model.DavinciText,
                    MaxTokens = 500,
                    Prompt = prompt,
                };
                var completions = await openAIAPI.Completions.CreateCompletionAsync(completionRequest);

                if (!completions.Completions.Any())//completions.Result.completions
                {
                    System.Diagnostics.Debug.WriteLine("No completions returned. Exit now");
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