using ChangeManagement.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagement.Api.Data;

public class ChangeManagementDbContext : DbContext
{
    public ChangeManagementDbContext(DbContextOptions<ChangeManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
    public DbSet<ChangeTask> ChangeTasks => Set<ChangeTask>();
    public DbSet<ChangeApproval> ChangeApprovals => Set<ChangeApproval>();
    public DbSet<ChangeAttachment> ChangeAttachments => Set<ChangeAttachment>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<ChangeType> ChangeTypes => Set<ChangeType>();
    public DbSet<ChangePriority> ChangePriorities => Set<ChangePriority>();
    public DbSet<ChangeStatus> ChangeStatuses => Set<ChangeStatus>();
    public DbSet<RiskLevel> RiskLevels => Set<RiskLevel>();
    public DbSet<ApprovalStatus> ApprovalStatuses => Set<ApprovalStatus>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

        if (!isSqlite)
        {
            modelBuilder.HasSequence<int>("ChangeNumberSeq", "cm").StartsAt(1000).IncrementsBy(1);
        }

        modelBuilder.HasSequence<int>("ChangeNumberSeq", "cm").StartsAt(1000).IncrementsBy(1);

        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.ToTable("ChangeRequest", "cm");
            entity.HasKey(e => e.ChangeRequestId);
            if (!isSqlite)
            {
                entity.Property(e => e.ChangeNumber).HasDefaultValueSql("NEXT VALUE FOR cm.ChangeNumberSeq");
            }
            else
            {
                entity.Property(e => e.ChangeNumber).ValueGeneratedOnAdd();
            }
            entity.HasIndex(e => e.ChangeNumber).IsUnique();

            entity.HasOne(e => e.ChangeType).WithMany(e => e.ChangeRequests).HasForeignKey(e => e.ChangeTypeId);
            entity.HasOne(e => e.Priority).WithMany(e => e.ChangeRequests).HasForeignKey(e => e.PriorityId);
            entity.HasOne(e => e.Status).WithMany(e => e.ChangeRequests).HasForeignKey(e => e.StatusId);
            entity.HasOne(e => e.RiskLevel).WithMany(e => e.ChangeRequests).HasForeignKey(e => e.RiskLevelId);
            entity.HasOne(e => e.RequestedByUser).WithMany(e => e.RequestedChanges).HasForeignKey(e => e.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedToUser).WithMany(e => e.AssignedChanges).HasForeignKey(e => e.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeTask>(entity =>
        {
            entity.ToTable("ChangeTask", "cm");
            entity.HasKey(e => e.ChangeTaskId);
            entity.HasOne(e => e.ChangeRequest).WithMany(e => e.ChangeTasks).HasForeignKey(e => e.ChangeRequestId);
            entity.HasOne(e => e.Status).WithMany(e => e.ChangeTasks).HasForeignKey(e => e.StatusId);
            entity.HasOne(e => e.AssignedToUser).WithMany(e => e.AssignedTasks).HasForeignKey(e => e.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeApproval>(entity =>
        {
            entity.ToTable("ChangeApproval", "cm");
            entity.HasKey(e => e.ChangeApprovalId);
            entity.HasOne(e => e.ChangeRequest).WithMany(e => e.ChangeApprovals).HasForeignKey(e => e.ChangeRequestId);
            entity.HasOne(e => e.ApprovalStatus).WithMany(e => e.ChangeApprovals).HasForeignKey(e => e.ApprovalStatusId);
            entity.HasOne(e => e.ApproverUser).WithMany(e => e.Approvals).HasForeignKey(e => e.ApproverUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChangeAttachment>(entity =>
        {
            entity.ToTable("ChangeAttachment", "cm");
            entity.HasKey(e => e.ChangeAttachmentId);
            entity.HasOne(e => e.ChangeRequest).WithMany(e => e.ChangeAttachments).HasForeignKey(e => e.ChangeRequestId);
            entity.HasOne(e => e.UploadedByUser).WithMany(e => e.UploadedAttachments).HasForeignKey(e => e.UploadedBy).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("Event", "audit");
            entity.HasKey(e => e.AuditEventId);
            entity.HasOne(e => e.EventType).WithMany(e => e.Events).HasForeignKey(e => e.EventTypeId);
        });

        modelBuilder.Entity<EventType>(entity => { entity.ToTable("EventType", "audit"); entity.HasKey(e => e.EventTypeId); });
        modelBuilder.Entity<ChangeType>(entity => { entity.ToTable("ChangeType", "ref"); entity.HasKey(e => e.ChangeTypeId); });
        modelBuilder.Entity<ChangePriority>(entity => { entity.ToTable("ChangePriority", "ref"); entity.HasKey(e => e.PriorityId); });
        modelBuilder.Entity<ChangeStatus>(entity => { entity.ToTable("ChangeStatus", "ref"); entity.HasKey(e => e.StatusId); });
        modelBuilder.Entity<RiskLevel>(entity => { entity.ToTable("RiskLevel", "ref"); entity.HasKey(e => e.RiskLevelId); });
        modelBuilder.Entity<ApprovalStatus>(entity => { entity.ToTable("ApprovalStatus", "ref"); entity.HasKey(e => e.ApprovalStatusId); });
        modelBuilder.Entity<User>(entity => { entity.ToTable("User", "cm"); entity.HasKey(e => e.UserId); });

        modelBuilder.Entity<ChangeType>().HasData(
            new ChangeType { ChangeTypeId = 1, Name = "Standard", Description = "Pre-approved repeatable change" },
            new ChangeType { ChangeTypeId = 2, Name = "Normal", Description = "Normal CAB-reviewed change" },
            new ChangeType { ChangeTypeId = 3, Name = "Emergency", Description = "Emergency expedited change" });

        modelBuilder.Entity<ChangePriority>().HasData(
            new ChangePriority { PriorityId = 1, Name = "Low", SortOrder = 1 },
            new ChangePriority { PriorityId = 2, Name = "Medium", SortOrder = 2 },
            new ChangePriority { PriorityId = 3, Name = "High", SortOrder = 3 },
            new ChangePriority { PriorityId = 4, Name = "Critical", SortOrder = 4 });

        modelBuilder.Entity<ChangeStatus>().HasData(
            new ChangeStatus { StatusId = 1, Name = "Draft", IsTerminal = false },
            new ChangeStatus { StatusId = 2, Name = "Submitted", IsTerminal = false },
            new ChangeStatus { StatusId = 3, Name = "Approved", IsTerminal = false },
            new ChangeStatus { StatusId = 4, Name = "Rejected", IsTerminal = true },
            new ChangeStatus { StatusId = 5, Name = "Completed", IsTerminal = true });

        modelBuilder.Entity<RiskLevel>().HasData(
            new RiskLevel { RiskLevelId = 1, Name = "Low", Score = 1 },
            new RiskLevel { RiskLevelId = 2, Name = "Medium", Score = 5 },
            new RiskLevel { RiskLevelId = 3, Name = "High", Score = 8 });

        modelBuilder.Entity<ApprovalStatus>().HasData(
            new ApprovalStatus { ApprovalStatusId = 1, Name = "Pending" },
            new ApprovalStatus { ApprovalStatusId = 2, Name = "Approved" },
            new ApprovalStatus { ApprovalStatusId = 3, Name = "Rejected" });

        modelBuilder.Entity<EventType>().HasData(
            new EventType { EventTypeId = 1, Name = "ChangeCreated", Description = "Change request created" },
            new EventType { EventTypeId = 2, Name = "ChangeUpdated", Description = "Change request updated" },
            new EventType { EventTypeId = 3, Name = "ChangeSubmitted", Description = "Change submitted for approval" },
            new EventType { EventTypeId = 4, Name = "ApprovalDecision", Description = "Approval decision recorded" },
            new EventType { EventTypeId = 5, Name = "AttachmentUploaded", Description = "Attachment uploaded" },
            new EventType { EventTypeId = 6, Name = "TemplateCreated", Description = "Template created" },
            new EventType { EventTypeId = 7, Name = "TemplateUpdated", Description = "Template updated" });
    }
}
