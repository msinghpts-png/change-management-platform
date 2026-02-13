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
    public DbSet<ChangeAttachment> ChangeAttachments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

            entity.HasMany(e => e.Attachments)
                .WithOne()
                .HasForeignKey(a => a.ChangeRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChangeApproval>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeRequestId).IsRequired();
            entity.Property(e => e.Approver).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);
        });

        modelBuilder.Entity<ChangeAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeRequestId).IsRequired();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(260);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(128);
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(1024);
            entity.Property(e => e.UploadedAt).IsRequired();
            entity.HasIndex(e => new { e.ChangeRequestId, e.FileName, e.UploadedAt });
        });
    }
}
