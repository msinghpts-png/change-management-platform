using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Data;

public class ChangeManagementDbContext : DbContext
{
    public ChangeManagementDbContext(DbContextOptions<ChangeManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
    public DbSet<ChangeApprover> ChangeApprovers => Set<ChangeApprover>();
    public DbSet<ChangeAttachment> ChangeAttachments => Set<ChangeAttachment>();
    public DbSet<ChangeTask> ChangeTasks => Set<ChangeTask>();
    public DbSet<ChangeTemplate> ChangeTemplates => Set<ChangeTemplate>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<ChangeType> ChangeTypes => Set<ChangeType>();
    public DbSet<ChangePriority> ChangePriorities => Set<ChangePriority>();
    public DbSet<ChangeStatus> ChangeStatuses => Set<ChangeStatus>();
    public DbSet<RiskLevel> RiskLevels => Set<RiskLevel>();


    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PreventHardDeleteOfChangeRequests();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PreventHardDeleteOfChangeRequests();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void PreventHardDeleteOfChangeRequests()
    {
        var hasHardDelete = ChangeTracker
            .Entries<ChangeRequest>()
            .Any(entry => entry.State == EntityState.Deleted);

        if (hasHardDelete)
        {
            throw new InvalidOperationException("Hard delete is not allowed for ChangeRequest. Use soft delete workflow.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", "cm");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Upn).HasMaxLength(256).IsRequired();
            entity.HasIndex(e => e.Upn).IsUnique();
            entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.ToTable("ChangeRequest", "cm");
            entity.HasKey(e => e.ChangeRequestId);
            entity.Property(e => e.ChangeNumber).IsRequired();
            entity.HasIndex(e => e.ChangeNumber).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.ChangeTypeId).IsRequired();
            entity.Property(e => e.PriorityId).IsRequired();
            entity.Property(e => e.StatusId).IsRequired();
            entity.Property(e => e.RiskLevelId).IsRequired();
            entity.Property(e => e.ImplementationGroup).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(200);
            entity.Property(e => e.Environment).HasMaxLength(200);
            entity.Property(e => e.ServiceSystem).HasMaxLength(200);
            entity.Property(e => e.ApprovalRequired).HasDefaultValue(false);
            entity.Property(e => e.ApprovalStrategy).HasMaxLength(50);
            entity.Property(e => e.DeletedReason).HasMaxLength(400);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasOne(e => e.RequestedByUser).WithMany(e => e.RequestedChanges).HasForeignKey(e => e.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedToUser).WithMany(e => e.AssignedChanges).HasForeignKey(e => e.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SubmittedByUser).WithMany(e => e.SubmittedChanges).HasForeignKey(e => e.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ApprovalRequesterUser).WithMany(e => e.ApprovalRequestedChanges).HasForeignKey(e => e.ApprovalRequesterUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.DeletedByUser).WithMany(e => e.DeletedChanges).HasForeignKey(e => e.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedByUser).WithMany(e => e.CreatedChanges).HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.UpdatedByUser).WithMany(e => e.UpdatedChanges).HasForeignKey(e => e.UpdatedBy).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ChangeType).WithMany(e => e.ChangeRequests).HasForeignKey(e => e.ChangeTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Priority).WithMany(e => e.ChangeRequests).HasForeignKey(e => e.PriorityId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Status).WithMany(e => e.ChangeRequests).HasForeignKey(e => e.StatusId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.RiskLevel).WithMany(e => e.ChangeRequests).HasForeignKey(e => e.RiskLevelId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeApprover>(entity =>
        {
            entity.ToTable("ChangeApprover", "cm");
            entity.HasKey(e => e.ChangeApproverId);
            entity.Property(e => e.ApprovalStatus).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.ChangeRequest).WithMany(e => e.ChangeApprovers).HasForeignKey(e => e.ChangeRequestId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ApproverUser).WithMany(e => e.ChangeApproverSelections).HasForeignKey(e => e.ApproverUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeAttachment>(entity =>
        {
            entity.ToTable("ChangeAttachment", "cm");
            entity.HasKey(e => e.ChangeAttachmentId);
            entity.Property(e => e.FileName).HasMaxLength(300).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileSizeBytes).IsRequired();
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.ChangeRequest).WithMany(e => e.ChangeAttachments).HasForeignKey(e => e.ChangeRequestId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.UploadedByUser).WithMany(e => e.UploadedAttachments).HasForeignKey(e => e.UploadedBy).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeTask>(entity =>
        {
            entity.ToTable("ChangeTask", "cm");
            entity.HasKey(e => e.ChangeTaskId);
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.ChangeRequest).WithMany(e => e.ChangeTasks).HasForeignKey(e => e.ChangeRequestId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssignedToUser).WithMany(e => e.AssignedTasks).HasForeignKey(e => e.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeTemplate>(entity =>
        {
            entity.ToTable("ChangeTemplate", "cm");
            entity.HasKey(e => e.TemplateId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ServiceSystem).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(200);
            entity.Property(e => e.Environment).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.CreatedByUser).WithMany(e => e.CreatedTemplates).HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.ToTable("EventType", "audit");
            entity.HasKey(e => e.EventTypeId);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(300);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("AuditEvent", "audit");
            entity.HasKey(e => e.AuditEventId);
            entity.Property(e => e.ActorUpn).HasMaxLength(256);
            entity.Property(e => e.SchemaName).HasMaxLength(50);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.EntityNumber).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.EventType).WithMany(e => e.Events).HasForeignKey(e => e.EventTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ActorUser).WithMany(e => e.AuditEvents).HasForeignKey(e => e.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeType>(entity =>
        {
            entity.ToTable("ChangeType", "ref");
            entity.HasKey(e => e.ChangeTypeId);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(300);
        });

        modelBuilder.Entity<ChangePriority>(entity =>
        {
            entity.ToTable("ChangePriority", "ref");
            entity.HasKey(e => e.PriorityId);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ChangeStatus>(entity =>
        {
            entity.ToTable("ChangeStatus", "ref");
            entity.HasKey(e => e.StatusId);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<RiskLevel>(entity =>
        {
            entity.ToTable("RiskLevel", "ref");
            entity.HasKey(e => e.RiskLevelId);
            entity.Property(e => e.Name).HasMaxLength(100);
        });
    }
}
