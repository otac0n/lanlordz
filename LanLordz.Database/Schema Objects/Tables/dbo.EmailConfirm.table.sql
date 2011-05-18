CREATE TABLE [dbo].[EmailConfirm]
(
    [Email] nvarchar(50) NOT NULL,
    [Key] uniqueidentifier NOT NULL,
    CONSTRAINT PK_EmailConfirm PRIMARY KEY CLUSTERED ([Email] ASC)
)
