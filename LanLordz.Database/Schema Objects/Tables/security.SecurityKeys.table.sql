CREATE TABLE [security].[SecurityKeys]
(
    [UserId] bigint NOT NULL,
    [Key] uniqueidentifier NOT NULL,
    [ExpirationDate] datetime NOT NULL,
    CONSTRAINT PK_SecurityKeys PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT FK_SecurityKeys_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION 
)
