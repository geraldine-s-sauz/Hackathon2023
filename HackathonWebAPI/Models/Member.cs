using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebAPI.Models
{
    public class Member
    {
        [Key] public string? MemberId { get; set; }

        [DisplayName("First Name")] public string? FirstName { get; set; }

        [DisplayName("Last Name")] public string? LastName { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }
}