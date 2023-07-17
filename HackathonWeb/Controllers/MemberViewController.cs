using HackathonWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;

namespace HackathonWeb.Controllers
{
    public class MemberViewController : Controller
    {
        private readonly Uri baseAddress = new("https://localhost:44337/api");
        private readonly HttpClient _httpClient;

        public MemberViewController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = baseAddress
            };
        }

        //GET
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            List<Member>? memberList;
            HttpResponseMessage response = await _httpClient.GetAsync(baseAddress + "/Member");
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("No data is found in memberList.");
            }
            else
            {
                string data = response.Content.ReadAsStringAsync().Result;
                memberList = JsonConvert.DeserializeObject<List<Member>>(data);
            }
            return View(memberList);
        }

        //POST
        public async Task<ActionResult<Member>> PostMember(Member model)
        {
            Member temp = model;
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Error creating the resource.");
            }
            else
            {
                temp.MemberId = Guid.NewGuid().ToString();
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(baseAddress + "/Member", temp);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("No data is found in memberList.");
                }
                else
                {
                    //return RedirectToAction("GetMembers", "MemberViewController");
                    //System.Diagnostics.Debug.WriteLine(Json(response));
                }
            }
            return View(temp);
        }
    }
}