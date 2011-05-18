CREATE TABLE [forums].[ThreadRead]
(
    [ThreadReadId] bigint NOT NULL IDENTITY,
    [ThreadId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [DateRead] datetime NOT NULL,
    CONSTRAINT PK_ThreadRead PRIMARY KEY CLUSTERED ([ThreadReadId] ASC),
    CONSTRAINT IX_ThreadRead_ThreadId_UserId UNIQUE ([ThreadId] DESC, [UserId] ASC),
    CONSTRAINT FK_ThreadRead_Threads FOREIGN KEY ([ThreadId]) REFERENCES [forums].[Threads] ([ThreadId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_ThreadRead_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
