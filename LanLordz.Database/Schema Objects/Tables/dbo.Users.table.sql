CREATE TABLE [dbo].[Users] (
    [UserId] bigint NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [PasswordHash] char(88) NOT NULL,
    [SecurityQuestion] nvarchar(100) NOT NULL,
    [SecurityAnswer] nvarchar(50) NOT NULL,
    [CreateDate] datetime NOT NULL,
    [Email] nvarchar(50) NOT NULL,
    [ShowEmail] bit NOT NULL,
    [ReceiveAdminEmail] bit NOT NULL,
    [Gender] char(1) NOT NULL,
    [ShowGender] bit NOT NULL,
    [IsEmailConfirmed] bit NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT IX_Users_Username UNIQUE ([Username] ASC),
    CONSTRAINT IX_Users_Email UNIQUE ([Email] ASC)
)
