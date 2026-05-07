using JobInterviewCore.Entities;
using JobInterviewCore.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobInterviewDataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Question
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Answer).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Difficulty).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Topic)
                .WithMany(t => t.Questions)
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.TopicId);
        });

        // Topic
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Attempt
        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AttemptedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Question)
                .WithMany(q => q.Attempts)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.QuestionId);
        });

        // Rating
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Question)
                .WithMany(q => q.Ratings)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.QuestionId);
        });

        // Bookmark 
        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Question)
                .WithMany(q => q.Bookmarks)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Egy kérdéshez max egy könyvjelző
            entity.HasIndex(e => e.QuestionId).IsUnique();
        });
    }
}
