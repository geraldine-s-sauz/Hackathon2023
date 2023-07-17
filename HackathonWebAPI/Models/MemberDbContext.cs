using Microsoft.EntityFrameworkCore;

namespace HackathonWebAPI.Models
{
    public class MemberDbContext : DbContext
    {
        public MemberDbContext(DbContextOptions<MemberDbContext> options) : base(options)
        {
        }

        public DbSet<Member> Member { get; set; }
    }
}