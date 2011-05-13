CREATE TABLE [dbo].[PollResponses](
    [PollResponseId] bigint IDENTITY NOT NULL,
    [PollId] bigint NOT NULL,
    [Label] nvarchar(60) NOT NULL,
    CONSTRAINT PK_PollResponses PRIMARY KEY CLUSTERED ([PollResponseId] ASC),
    CONSTRAINT FK_PollResponses_Polls FOREIGN KEY ([PollId]) REFERENCES [dbo].[Polls] ([PollId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
)
