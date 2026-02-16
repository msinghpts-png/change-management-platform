using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Data;

public class ChangeManagementDbContext : DbContext
{
    public ChangeManagementDbContext(DbContextOptions<ChangeManagementDbContext> options) : base(options)
    {
    }

    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
    public DbSet<ChangeApproval> ChangeApprovals => Set<ChangeApproval>();
    public DbSet<ChangeTask> ChangeTasks => Set<ChangeTask>();
    public DbSet<ChangeTemplate> ChangeTemplates => Set<ChangeTemplate>();
    public DbSet<ChangeAttachment> ChangeAttachments => Set<ChangeAttachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.ToTable("ChangeRequest", "cm");
            entity.HasKey(x => x.ChangeId);
            entity.Property(x => x.ChangeNumber).ValueGeneratedOnAdd();
            entity.HasIndex(x => x.ChangeNumber).IsUnique();

            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.CreatedChanges)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedToUser)
                .WithMany(x => x.AssignedChanges)
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeApproval>(entity =>
        {
            entity.ToTable("ChangeApproval", "cm");
            entity.HasKey(x => x.ChangeApprovalId);
            entity.HasOne(x => x.ChangeRequest).WithMany(x => x.ChangeApprovals).HasForeignKey(x => x.ChangeId);
            entity.HasOne(x => x.CabUser).WithMany().HasForeignKey(x => x.CabUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeTask>(entity =>
        {
            entity.ToTable("ChangeTask", "cm");
            entity.HasKey(x => x.ChangeTaskId);
            entity.HasOne(x => x.ChangeRequest).WithMany(x => x.ChangeTasks).HasForeignKey(x => x.ChangeId);
            entity.HasOne(x => x.AssignedToUser).WithMany().HasForeignKey(x => x.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeTemplate>(entity =>
        {
            entity.ToTable("ChangeTemplate", "cm");
            entity.HasKey(x => x.ChangeTemplateId);
        });

        modelBuilder.Entity<ChangeAttachment>(entity =>
        {
            entity.ToTable("ChangeAttachment", "cm");
            entity.HasKey(x => x.ChangeAttachmentId);
            entity.HasOne(x => x.ChangeRequest).WithMany(x => x.ChangeAttachments).HasForeignKey(x => x.ChangeId);
            entity.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog", "audit");
            entity.HasKey(x => x.AuditLogId);
            entity.HasOne(x => x.ChangeRequest).WithMany().HasForeignKey(x => x.ChangeId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", "cm");
            entity.HasKey(x => x.UserId);
            entity.HasIndex(x => x.Upn).IsUnique();
        });
    }
}
