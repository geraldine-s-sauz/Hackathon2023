using HackathonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HackathonWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssessmentController : ControllerBase
    {
        private readonly HackathonDbContext _dataContext;

        public AssessmentController(HackathonDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        // GET: api/<MemberController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentQuestions>>> GetAssessmentQuestions()
        {
            if (_dataContext == null)
            {
                return BadRequest("No results found.");
            }
            else
            {
                return await _dataContext.AssessmentQuestions.ToListAsync();
            }
        }

        // GET api/<MemberController>/5
        [HttpGet("{questionId}")]
        public async Task<ActionResult<IEnumerable<AssessmentQuestions>>> GetMemberById([FromRoute] string questionId)
        {
            AssessmentQuestions? temp = await _dataContext.AssessmentQuestions.FindAsync(questionId);
            if (temp is null || _dataContext is null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(temp);
            }
        }

        // POST api/<MemberController>
        [HttpPost]
        public async Task<ActionResult<AssessmentQuestions>> PostMember()
        {
            string tempId = Guid.NewGuid().ToString();
            AssessmentQuestions temp = new()
            {
                QuestionId = tempId,
                Question1 = "How can I help you?",
                Question3 = "When did the headaches start?",
                Question2 = "Any specific triggers for these headaches?"
            };
            _dataContext.AssessmentQuestions.Add(temp);
            await _dataContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMemberById), new { memberId = tempId }, temp);
        }
    }
}