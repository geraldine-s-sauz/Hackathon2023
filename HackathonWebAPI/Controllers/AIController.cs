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

            string? tempTranscript = "INT. COMMUNITY ASSESSMENT - DAY\r\n\r\n\r\nLisa (HCW): Hi, good day. I am your Health Coordinator Lisa David. \r\n\r\nPat: Hi Lisa, I am Pat.\r\n\r\nLisa (HCW): How are you today?\r\n\r\nPat: Not so good, I am feeling a bit under the weather today.\r\n\r\nLisa (HCW): I'm sorry to hear that but don't worry, I'm here to assess you and see how we can help.\r\n\r\nPat: Great, thank you\r\n\r\nLisa (HCW): Okay, first, may I know the Patient's Full Name?\r\n\r\nPat: My name is Pat Smith.\r\n\r\nLisa: May I know the Medical Conditions you're dealing with right now?\r\n\r\nPat: I'm having migraines today, could be due to lack of sleep. I usually just manage them with rest and water but\r\nit is worse this day for some reason.\r\n\r\nLisa (HCW): I'm sorry to hear that. Any particular reason for the lack of sleep? \r\n\r\nPat: Yeah, just stressed out because of a big presentation coming up.\r\n\r\nLisa: That's understandable. Do you have other Medical History that we should know of?\r\n\r\nPat: Yes, I also have hypertension and arthritis. \r\n\r\nLisa (HCW) : By any chance are you taking any Medication for these illness?\r\n\r\nPat: Yes, I am taking Amlodipine for my blood pressure and Ibuprofen for when my joints act up.\r\n\r\nLisa: Alright, how about your Vaccine History?\r\n\r\nPat:  I have just gotten my third Pfizer Covid booster a week ago.\r\n\r\nLisa (HCW): Oh, that's great. How are you feeling? Any side effects?\r\n\r\nPat: I'm feeling good, my left arm felt a little heavy but other than that, I did not feel any side effects.\r\n\r\nLisa: How about your first and second vaccine?\r\n\r\nPat (HCW): I first got it last June 2022 and then the second one on January 2023. Luckily I did not feel any side effects either.\r\n\r\nLisa (HCW) : That's good to hear. So that is all for our assessment, any additional information that you might have missed?\r\n\r\nPat: Not that I think of, but if anything else comes up I 'll let you know.\r\n\r\nLisa (HCW): Alright then, I think we are all set here. Thank you so much for your time. Have a great day.\r\n\r\nPat: Thank you too. Have a great day.\r\n\r\nLisa (HCW): To summarize, the patient is suffering from migraine due to stress and lack of sleep. She's taking Amlodipine for hypertension and has history of arthritis. \r\nShe completed her 3rd doze of covid vaccine a week ago with no side effects.";
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