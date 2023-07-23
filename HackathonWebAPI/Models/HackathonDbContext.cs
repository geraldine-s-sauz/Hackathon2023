using Microsoft.EntityFrameworkCore;

namespace HackathonWebAPI.Models
{
    public class HackathonDbContext : DbContext
    {
        public HackathonDbContext(DbContextOptions<HackathonDbContext> options) : base(options)
        {
        }

        public DbSet<Member> Member { get; set; }
        public DbSet<AssessmentQuestions> AssessmentQuestions { get; set; }
    }
}