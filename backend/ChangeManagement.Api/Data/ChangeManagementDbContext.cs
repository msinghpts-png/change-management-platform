using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Data;

public class ChangeManagementDbContext : DbContext
{
    public ChangeManagementDbContext(DbContextOptions<ChangeManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChangeRequest> ChangeRequests { get; set; } = null!;
    public DbSet<ChangeApproval> ChangeApprovals { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ChangeRequest configuration
        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.RiskLevel).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasMany(e => e.Approvals)
                .WithOne()
                .HasForeignKey(a => a.ChangeRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChangeApproval configuration
        modelBuilder.Entity<ChangeApproval>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeRequestId).IsRequired();
            entity.Property(e => e.Approver).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);
        });
    }
}
