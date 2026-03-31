using Microsoft.EntityFrameworkCore;
using Pethub.Models;

namespace Pethub.Data
{
    public class PethubContext : DbContext
    {
        public PethubContext(DbContextOptions<PethubContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Account { get; set; } = default!;
        public DbSet<Post> Post { get; set; } = default!;
        public DbSet<Report> Report { get; set; } = default!;
        public DbSet<Pet> Pet { get; set; } = default!;
        public DbSet<ActivityLog> ActivityLog { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Post → Account: restrict so deleting an account does not cascade-delete its posts
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Account)
                .WithMany()
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Report → Post: cascade so deleting a post removes all its reports
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Report → Account (reporter): restrict to avoid cascade conflict with the Post → Account path
            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Account)
                .WithMany()
                .HasForeignKey(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}