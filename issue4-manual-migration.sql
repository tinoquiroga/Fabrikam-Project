-- Manual migration script for Issue #4
-- This script safely adds the missing database fields required by GitHub Issue #4

-- Ensure migrations history table exists
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT 'Created __EFMigrationsHistory table';
END;

-- Mark existing migrations as applied (since tables already exist)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727042608_InitialCreateIdentitySchema', N'9.0.7');
    PRINT 'Marked InitialCreateIdentitySchema as applied';
END;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250727042732_FixRoleSeedData')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727042732_FixRoleSeedData', N'9.0.7');
    PRINT 'Marked FixRoleSeedData as applied';
END;

-- Issue #4: Add new columns to FabUsers table for Entra External ID integration and audit trail
PRINT 'Adding Issue #4 columns to FabUsers table...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUsers') AND name = 'EntraObjectId')
BEGIN
    ALTER TABLE [FabUsers] ADD [EntraObjectId] nvarchar(max) NULL;
    PRINT '✓ Added EntraObjectId column';
END
ELSE
    PRINT '• EntraObjectId column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUsers') AND name = 'CompanyName')
BEGIN
    ALTER TABLE [FabUsers] ADD [CompanyName] nvarchar(max) NOT NULL DEFAULT N'';
    PRINT '✓ Added CompanyName column';
END
ELSE
    PRINT '• CompanyName column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUsers') AND name = 'DeletedDate')
BEGIN
    ALTER TABLE [FabUsers] ADD [DeletedDate] datetime2 NULL;
    PRINT '✓ Added DeletedDate column';
END
ELSE
    PRINT '• DeletedDate column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUsers') AND name = 'UpdatedDate')
BEGIN
    ALTER TABLE [FabUsers] ADD [UpdatedDate] datetime2 NOT NULL DEFAULT '1900-01-01T00:00:00.0000000';
    PRINT '✓ Added UpdatedDate column';
END
ELSE
    PRINT '• UpdatedDate column already exists';

-- Issue #4: Add audit columns to FabUserRoles table
PRINT 'Adding Issue #4 audit columns to FabUserRoles table...';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUserRoles') AND name = 'AssignedAt')
BEGIN
    ALTER TABLE [FabUserRoles] ADD [AssignedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
    PRINT '✓ Added AssignedAt column';
END
ELSE
    PRINT '• AssignedAt column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUserRoles') AND name = 'AssignedBy')
BEGIN
    ALTER TABLE [FabUserRoles] ADD [AssignedBy] nvarchar(256) NULL;
    PRINT '✓ Added AssignedBy column';
END
ELSE
    PRINT '• AssignedBy column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUserRoles') AND name = 'AssignmentNotes')
BEGIN
    ALTER TABLE [FabUserRoles] ADD [AssignmentNotes] nvarchar(500) NULL;
    PRINT '✓ Added AssignmentNotes column';
END
ELSE
    PRINT '• AssignmentNotes column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUserRoles') AND name = 'ExpiresAt')
BEGIN
    ALTER TABLE [FabUserRoles] ADD [ExpiresAt] datetime2 NULL;
    PRINT '✓ Added ExpiresAt column';
END
ELSE
    PRINT '• ExpiresAt column already exists';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FabUserRoles') AND name = 'IsActive')
BEGIN
    ALTER TABLE [FabUserRoles] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
    PRINT '✓ Added IsActive column';
END
ELSE
    PRINT '• IsActive column already exists';

-- Create performance indexes for Issue #4 audit fields
PRINT 'Creating performance indexes...';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FabUserRoles') AND name = 'IX_FabUserRoles_AssignedAt')
BEGIN
    CREATE INDEX [IX_FabUserRoles_AssignedAt] ON [FabUserRoles] ([AssignedAt]);
    PRINT '✓ Created index IX_FabUserRoles_AssignedAt';
END
ELSE
    PRINT '• Index IX_FabUserRoles_AssignedAt already exists';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FabUserRoles') AND name = 'IX_FabUserRoles_ExpiresAt')
BEGIN
    CREATE INDEX [IX_FabUserRoles_ExpiresAt] ON [FabUserRoles] ([ExpiresAt]);
    PRINT '✓ Created index IX_FabUserRoles_ExpiresAt';
END
ELSE
    PRINT '• Index IX_FabUserRoles_ExpiresAt already exists';

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('FabUserRoles') AND name = 'IX_FabUserRoles_IsActive')
BEGIN
    CREATE INDEX [IX_FabUserRoles_IsActive] ON [FabUserRoles] ([IsActive]);
    PRINT '✓ Created index IX_FabUserRoles_IsActive';
END
ELSE
    PRINT '• Index IX_FabUserRoles_IsActive already exists';

-- Mark Issue #4 migration as applied
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250727051059_CompleteIssue4WithEntraID')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727051059_CompleteIssue4WithEntraID', N'9.0.7');
    PRINT '✓ Marked CompleteIssue4WithEntraID migration as applied';
END
ELSE
    PRINT '• CompleteIssue4WithEntraID migration already marked as applied';

PRINT 'Issue #4 database schema migration completed successfully!';
PRINT 'All required fields for Microsoft Entra External ID integration and audit trail have been added.';
