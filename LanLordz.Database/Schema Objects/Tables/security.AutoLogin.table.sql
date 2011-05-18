CREATE TABLE [security].[AutoLogin]
(
    [Key] uniqueidentifier NOT NULL,
    [UserId] bigint NOT NULL,
    [ExpirationDate] datetime NOT NULL,
    CONSTRAINT PK_AutoLogin PRIMARY KEY CLUSTERED ([Key] ASC),
    CONSTRAINT FK_AutoLogin_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
