using Microsoft.EntityFrameworkCore;
using NewsAggregator.Models;

namespace NewsAggregator.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
           {
            modelBuilder.Entity<Article>()
                .HasIndex(a => a.PublishedAt);

            modelBuilder.Entity<Article>()
                .HasIndex(a => a.Url)
                .IsUnique();
        }
    }
}