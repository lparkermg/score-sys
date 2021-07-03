
using Microsoft.EntityFrameworkCore;

namespace ScoreSys.Entities
{
    public sealed class GameViewContext : DbContext
    {
        public GameViewContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<GameView> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameView>().HasKey(s => s.Id);
        }
    }
}
