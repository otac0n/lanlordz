CREATE TABLE [dbo].[Polls]
(
    [PollId] bigint NOT NULL IDENTITY,
    [CreatorUserId] bigint NOT NULL,
    [Title] nvarchar(60) NOT NULL,
    [IsPrivate] bit NOT NULL,
    [IsMultiAnswer] bit NOT NULL,
    [Text] ntext NOT NULL,
    CONSTRAINT PK_Polls PRIMARY KEY CLUSTERED ([PollId] ASC),
    CONSTRAINT FK_Polls_CreatorUsers FOREIGN KEY ([CreatorUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
