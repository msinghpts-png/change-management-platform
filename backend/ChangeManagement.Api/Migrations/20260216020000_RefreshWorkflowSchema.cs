using ChangeManagement.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeManagement.Api.Migrations;

[DbContext(typeof(ChangeManagementDbContext))]
[Migration("20260216020000_RefreshWorkflowSchema")]
public partial class RefreshWorkflowSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF SCHEMA_ID('cm') IS NULL EXEC('CREATE SCHEMA [cm]');
IF SCHEMA_ID('audit') IS NULL EXEC('CREATE SCHEMA [audit]');

IF OBJECT_ID('[cm].[ChangeApproval]', 'U') IS NOT NULL DROP TABLE [cm].[ChangeApproval];
IF OBJECT_ID('[cm].[ChangeAttachment]', 'U') IS NOT NULL DROP TABLE [cm].[ChangeAttachment];
IF OBJECT_ID('[cm].[ChangeTask]', 'U') IS NOT NULL DROP TABLE [cm].[ChangeTask];
IF OBJECT_ID('[audit].[Event]', 'U') IS NOT NULL DROP TABLE [audit].[Event];
IF OBJECT_ID('[audit].[AuditLog]', 'U') IS NOT NULL DROP TABLE [audit].[AuditLog];
IF OBJECT_ID('[cm].[ChangeTemplate]', 'U') IS NOT NULL DROP TABLE [cm].[ChangeTemplate];
IF OBJECT_ID('[cm].[ChangeRequest]', 'U') IS NOT NULL DROP TABLE [cm].[ChangeRequest];

CREATE TABLE [cm].[ChangeRequest](
    [ChangeId] uniqueidentifier NOT NULL PRIMARY KEY,
    [ChangeNumber] int IDENTITY(1000,1) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [ChangeType] int NOT NULL,
    [RiskLevel] int NOT NULL,
    [Status] int NOT NULL,
    [ImpactDescription] nvarchar(max) NOT NULL,
    [RollbackPlan] nvarchar(max) NOT NULL,
    [ImplementationDate] datetime2 NULL,
    [ImplementationStartDate] datetime2 NULL,
    [CompletedDate] datetime2 NULL,
    [CreatedByUserId] uniqueidentifier NOT NULL,
    [AssignedToUserId] uniqueidentifier NULL,
    [ApprovedDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [FK_ChangeRequest_User_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [cm].[User]([UserId]),
    CONSTRAINT [FK_ChangeRequest_User_AssignedTo] FOREIGN KEY ([AssignedToUserId]) REFERENCES [cm].[User]([UserId])
);
CREATE UNIQUE INDEX [IX_ChangeRequest_ChangeNumber] ON [cm].[ChangeRequest]([ChangeNumber]);

CREATE TABLE [cm].[ChangeApproval](
    [ChangeApprovalId] uniqueidentifier NOT NULL PRIMARY KEY,
    [ChangeId] uniqueidentifier NOT NULL,
    [CabUserId] uniqueidentifier NOT NULL,
    [IsApproved] bit NOT NULL,
    [Comments] nvarchar(max) NOT NULL,
    [DecisionDate] datetime2 NOT NULL,
    CONSTRAINT [FK_ChangeApproval_ChangeRequest] FOREIGN KEY ([ChangeId]) REFERENCES [cm].[ChangeRequest]([ChangeId]),
    CONSTRAINT [FK_ChangeApproval_User] FOREIGN KEY ([CabUserId]) REFERENCES [cm].[User]([UserId])
);

CREATE TABLE [cm].[ChangeTask](
    [ChangeTaskId] uniqueidentifier NOT NULL PRIMARY KEY,
    [ChangeId] uniqueidentifier NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [AssignedToUserId] uniqueidentifier NULL,
    [DueDate] datetime2 NULL,
    [CompletedDate] datetime2 NULL,
    CONSTRAINT [FK_ChangeTask_ChangeRequest] FOREIGN KEY ([ChangeId]) REFERENCES [cm].[ChangeRequest]([ChangeId]),
    CONSTRAINT [FK_ChangeTask_User] FOREIGN KEY ([AssignedToUserId]) REFERENCES [cm].[User]([UserId])
);

CREATE TABLE [cm].[ChangeTemplate](
    [ChangeTemplateId] uniqueidentifier NOT NULL PRIMARY KEY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [ChangeType] int NOT NULL,
    [RiskLevel] int NOT NULL,
    [ImpactDescription] nvarchar(max) NOT NULL,
    [RollbackPlan] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL
);

CREATE TABLE [cm].[ChangeAttachment](
    [ChangeAttachmentId] uniqueidentifier NOT NULL PRIMARY KEY,
    [ChangeId] uniqueidentifier NOT NULL,
    [FileName] nvarchar(260) NOT NULL,
    [ContentType] nvarchar(100) NOT NULL,
    [FilePath] nvarchar(max) NOT NULL,
    [FileSizeBytes] bigint NOT NULL,
    [UploadedAt] datetime2 NOT NULL,
    [UploadedByUserId] uniqueidentifier NOT NULL,
    CONSTRAINT [FK_ChangeAttachment_ChangeRequest] FOREIGN KEY ([ChangeId]) REFERENCES [cm].[ChangeRequest]([ChangeId]),
    CONSTRAINT [FK_ChangeAttachment_User] FOREIGN KEY ([UploadedByUserId]) REFERENCES [cm].[User]([UserId])
);

CREATE TABLE [audit].[AuditLog](
    [AuditLogId] uniqueidentifier NOT NULL PRIMARY KEY,
    [ChangeId] uniqueidentifier NULL,
    [ActorUserId] uniqueidentifier NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [Details] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [FK_AuditLog_ChangeRequest] FOREIGN KEY ([ChangeId]) REFERENCES [cm].[ChangeRequest]([ChangeId]),
    CONSTRAINT [FK_AuditLog_User] FOREIGN KEY ([ActorUserId]) REFERENCES [cm].[User]([UserId])
);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
