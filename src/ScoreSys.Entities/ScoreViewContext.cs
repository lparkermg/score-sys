
using Microsoft.EntityFrameworkCore;

namespace ScoreSys.Entities
{
    public sealed class ScoreViewContext : DbContext
    {
        public ScoreViewContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ScoreView> Scores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScoreView>().HasKey(s => s.Id);
            modelBuilder.Entity<ScoreView>().HasIndex(s => s.Id);
        }
    }
}
