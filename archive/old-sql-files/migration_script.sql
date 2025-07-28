IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [Customers] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [Phone] nvarchar(20) NULL,
        [Address] nvarchar(200) NULL,
        [City] nvarchar(100) NULL,
        [State] nvarchar(10) NULL,
        [ZipCode] nvarchar(20) NULL,
        [Region] nvarchar(50) NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [FabRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [Priority] int NOT NULL DEFAULT 100,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_FabRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [ModelNumber] nvarchar(50) NOT NULL,
        [Category] int NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [StockQuantity] int NOT NULL,
        [ReorderLevel] int NOT NULL,
        [Dimensions] nvarchar(50) NULL,
        [SquareFeet] float NULL,
        [Bedrooms] int NULL,
        [Bathrooms] int NULL,
        [DeliveryDaysEstimate] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [FabUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [LastActiveDate] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CustomerId] int NULL,
        [NotificationPreferences] nvarchar(100) NOT NULL DEFAULT N'email',
        [IsAdmin] bit NOT NULL DEFAULT CAST(0 AS bit),
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_FabUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FabUsers_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [OrderNumber] nvarchar(20) NOT NULL,
        [Status] int NOT NULL,
        [OrderDate] datetime2 NOT NULL,
        [ShippedDate] datetime2 NULL,
        [DeliveredDate] datetime2 NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [Tax] decimal(18,2) NOT NULL,
        [Shipping] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [ShippingAddress] nvarchar(200) NULL,
        [ShippingCity] nvarchar(100) NULL,
        [ShippingState] nvarchar(10) NULL,
        [ShippingZipCode] nvarchar(20) NULL,
        [Region] nvarchar(50) NULL,
        [Notes] nvarchar(500) NULL,
        [LastUpdated] datetime2 NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [FabRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_FabRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FabRoleClaims_FabRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [FabRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [FabUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [GrantedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
        [GrantedBy] nvarchar(256) NULL,
        [ExpiresDate] datetime2 NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_FabUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FabUserClaims_FabUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [FabUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [FabUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_FabUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_FabUserLogins_FabUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [FabUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [FabUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_FabUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_FabUserRoles_FabRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [FabRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_FabUserRoles_FabUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [FabUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [FabUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_FabUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_FabUserTokens_FabUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [FabUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [CustomOptions] nvarchar(200) NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [SupportTickets] (
        [Id] int NOT NULL IDENTITY,
        [TicketNumber] nvarchar(20) NOT NULL,
        [CustomerId] int NOT NULL,
        [OrderId] int NULL,
        [Subject] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [Priority] int NOT NULL,
        [Category] int NOT NULL,
        [AssignedTo] nvarchar(100) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ResolvedDate] datetime2 NULL,
        [LastUpdated] datetime2 NOT NULL,
        [Region] nvarchar(50) NULL,
        CONSTRAINT [PK_SupportTickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportTickets_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SupportTickets_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE TABLE [TicketNotes] (
        [Id] int NOT NULL IDENTITY,
        [TicketId] int NOT NULL,
        [Note] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [IsInternal] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_TicketNotes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TicketNotes_SupportTickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [SupportTickets] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'CreatedDate', N'Description', N'IsActive', N'Name', N'NormalizedName', N'Priority') AND [object_id] = OBJECT_ID(N'[FabRoles]'))
        SET IDENTITY_INSERT [FabRoles] ON;
    EXEC(N'INSERT INTO [FabRoles] ([Id], [ConcurrencyStamp], [CreatedDate], [Description], [IsActive], [Name], [NormalizedName], [Priority])
    VALUES (N''1'', NULL, ''2025-07-27T04:26:07.6571324Z'', N''Full system access and user management'', CAST(1 AS bit), N''Administrator'', N''ADMINISTRATOR'', 1),
    (N''2'', NULL, ''2025-07-27T04:26:07.6571436Z'', N''Standard user access to business features'', CAST(1 AS bit), N''User'', N''USER'', 100),
    (N''3'', NULL, ''2025-07-27T04:26:07.6571438Z'', N''Management access to reports and customer data'', CAST(1 AS bit), N''Manager'', N''MANAGER'', 50)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'CreatedDate', N'Description', N'IsActive', N'Name', N'NormalizedName', N'Priority') AND [object_id] = OBJECT_ID(N'[FabRoles]'))
        SET IDENTITY_INSERT [FabRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabRoleClaims_RoleId] ON [FabRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabRoles_IsActive] ON [FabRoles] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabRoles_Priority] ON [FabRoles] ([Priority]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [FabRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabUserClaims_ExpiresDate] ON [FabUserClaims] ([ExpiresDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabUserClaims_IsActive] ON [FabUserClaims] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabUserClaims_UserId] ON [FabUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabUserLogins_UserId] ON [FabUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabUserRoles_RoleId] ON [FabUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [FabUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabUsers_CustomerId] ON [FabUsers] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_FabUsers_Email] ON [FabUsers] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabUsers_FirstName_LastName] ON [FabUsers] ([FirstName], [LastName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_FabUsers_IsActive] ON [FabUsers] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [FabUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_Orders_CustomerId_OrderDate] ON [Orders] ([CustomerId], [OrderDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_Orders_Status] ON [Orders] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_Products_Category] ON [Products] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_ModelNumber] ON [Products] ([ModelNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_CreatedDate] ON [SupportTickets] ([CreatedDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_CustomerId_Status] ON [SupportTickets] ([CustomerId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_OrderId] ON [SupportTickets] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SupportTickets_TicketNumber] ON [SupportTickets] ([TicketNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    CREATE INDEX [IX_TicketNotes_TicketId] ON [TicketNotes] ([TicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042608_InitialCreateIdentitySchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727042608_InitialCreateIdentitySchema', N'9.0.7');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042732_FixRoleSeedData'
)
BEGIN
    EXEC(N'UPDATE [FabRoles] SET [CreatedDate] = ''2025-01-01T00:00:00.0000000Z''
    WHERE [Id] = N''1'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042732_FixRoleSeedData'
)
BEGIN
    EXEC(N'UPDATE [FabRoles] SET [CreatedDate] = ''2025-01-01T00:00:00.0000000Z''
    WHERE [Id] = N''2'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042732_FixRoleSeedData'
)
BEGIN
    EXEC(N'UPDATE [FabRoles] SET [CreatedDate] = ''2025-01-01T00:00:00.0000000Z''
    WHERE [Id] = N''3'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727042732_FixRoleSeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727042732_FixRoleSeedData', N'9.0.7');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUsers] ADD [AzureB2CObjectId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUsers] ADD [CompanyName] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUsers] ADD [DeletedDate] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUsers] ADD [UpdatedDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUserRoles] ADD [AssignedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUserRoles] ADD [AssignedBy] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUserRoles] ADD [AssignmentNotes] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUserRoles] ADD [ExpiresAt] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    ALTER TABLE [FabUserRoles] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    CREATE INDEX [IX_FabUserRoles_AssignedAt] ON [FabUserRoles] ([AssignedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    CREATE INDEX [IX_FabUserRoles_ExpiresAt] ON [FabUserRoles] ([ExpiresAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    CREATE INDEX [IX_FabUserRoles_IsActive] ON [FabUserRoles] ([IsActive]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727045519_CompleteIssue4DatabaseSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727045519_CompleteIssue4DatabaseSchema', N'9.0.7');
END;

COMMIT;
GO

