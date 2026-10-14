
using Microsoft.EntityFrameworkCore;
using WorkRequest.Api.Models;

namespace WorkRequest.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkRequestItem> WorkRequests
        => Set<WorkRequestItem>();

    public DbSet<WorkRequestNote> WorkRequestNotes
        => Set<WorkRequestNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WorkRequestItem>(entity =>
        {
            entity.HasKey(w => w.Id);

            entity.HasMany(w => w.Notes)
                .WithOne(n => n.WorkRequestItem)
                .HasForeignKey(n => n.WorkRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(w => new { w.Status, w.CreatedDate });
            entity.HasIndex(w => w.ClientName);
        });

        modelBuilder.Entity<WorkRequestNote>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.HasIndex(n => n.WorkRequestId);
        });
    }
}
