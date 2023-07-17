using HackathonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HackathonWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly MemberDbContext _dataContext;

        public MemberController(MemberDbContext dataContext)
        {
            _dataContext = dataContext;
        }

        // GET: api/<MemberController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> Get()
        {
            if (_dataContext == null)
            {
                return BadRequest("No results found.");
            }
            else
            {
                return await _dataContext.Member.ToListAsync();
            }
        }

        // GET api/<MemberController>/5
        [HttpGet("{memberId}")]
        public async Task<ActionResult<IEnumerable<Member>>> GetMemberById([FromRoute] string memberId)
        {
            Member? temp = await _dataContext.Member.FindAsync(memberId);
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
        public async Task<ActionResult<Member>> PostMember([FromBody] Member temp)
        {
            temp.MemberId = Guid.NewGuid().ToString();
            _dataContext.Member.Add(temp);
            await _dataContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetMemberById), new { memberId = temp.MemberId }, temp);
        }

        //// PUT api/<MemberController>/5
        [HttpPut]
        public async Task<ActionResult<Member>> UpdateBrand([FromBody] Member temp)
        {
            if (!IsDataExist(temp.MemberId!))
            {
                return BadRequest(ModelState);
            }
            else
            {
                _dataContext.Entry(temp).State = EntityState.Modified;

                try
                {
                    await _dataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IsDataExist(temp.MemberId!))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    };
                }
                return Ok(temp);
            }
        }

        //// DELETE api/<MemberController>/5
        [HttpDelete("{memberId}")]
        public async Task<ActionResult<Member>> DeleteMember([FromRoute] string memberId)
        {
            if (!IsDataExist(memberId))
            {
                return NotFound();
            }
            else
            {
                Member? temp = await _dataContext.Member.FindAsync(memberId);
                if (temp == null)
                {
                    return NotFound(ModelState);
                }
                else
                {
                    _dataContext.Member.Remove(temp);
                    await _dataContext.SaveChangesAsync();
                    return Ok(temp.FirstName + " " + temp.LastName + " has been deleted.");
                }
            }
        }

        //checks wether the data exists in the table or not.
        private bool IsDataExist(String Id)
        {
            return (_dataContext.Member?.Any(x => x.MemberId == Id)).GetValueOrDefault();
        }
    }
}