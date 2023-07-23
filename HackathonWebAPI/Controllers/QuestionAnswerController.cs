using HackathonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
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

        //[HttpGet]
        //public async Task<ActionResult<AssessmentController>> GetQnA(string Question, string Answer)
        //{
        //    var configuration = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("local.appsettings.json")
        //        .Build();

        //    string? temp = configuration.GetSection("ApiKeys").GetSection("ApiKey_OpenAI").Value;

        //    return null;
        //}
    }
}