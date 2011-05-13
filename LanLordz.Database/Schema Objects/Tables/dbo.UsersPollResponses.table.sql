CREATE TABLE [dbo].[UsersPollResponses]
(
    [UserPollResponseId] [bigint] IDENTITY(1,1) NOT NULL,
    [UserId] [bigint] NOT NULL,
    [PollResponseId] [bigint] NOT NULL,
    [IsSelected] [bit] NOT NULL,
    CONSTRAINT PK_UsersPollResponses PRIMARY KEY CLUSTERED ([UserPollResponseId] ASC),
    CONSTRAINT FK_UsersPollResponses_PollResponses FOREIGN KEY ([PollResponseId]) REFERENCES [dbo].[PollResponses] ([PollResponseId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_UsersPollResponses_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT IX_UsersPollResponses_UserId_PollResponseId UNIQUE ([UserId] ASC, [PollResponseId] ASC)
)
