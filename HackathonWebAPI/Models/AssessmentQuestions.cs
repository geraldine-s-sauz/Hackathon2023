using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebAPI.Models
{
    public class AssessmentQuestions
    {
        [Key] public string? QuestionId { get; set; }
        public string? Question1 { get; set; }
        public string? Question2 { get; set; }
        public string? Question3 { get; set; }
    }
}