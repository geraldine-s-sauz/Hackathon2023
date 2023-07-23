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
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230717224606_InitialHackathon')
BEGIN
    CREATE TABLE [AssessmentQuestions] (
        [QuestionId] nvarchar(450) NOT NULL,
        [Question1] nvarchar(max) NULL,
        [Question2] nvarchar(max) NULL,
        [Question3] nvarchar(max) NULL,
        CONSTRAINT [PK_AssessmentQuestions] PRIMARY KEY ([QuestionId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230717224606_InitialHackathon')
BEGIN
    CREATE TABLE [Member] (
        [MemberId] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [CreatedDate] datetime2 NULL,
        CONSTRAINT [PK_Member] PRIMARY KEY ([MemberId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230717224606_InitialHackathon')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230717224606_InitialHackathon', N'7.0.9');
END;
GO

COMMIT;
GO

