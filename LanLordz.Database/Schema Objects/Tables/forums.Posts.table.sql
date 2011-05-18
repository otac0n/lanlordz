CREATE TABLE [forums].[Posts]
(
    [PostId] bigint NOT NULL IDENTITY,
    [ThreadId] bigint NOT NULL,
    [ResponseToPostId] bigint NULL,
    [UserId] bigint NOT NULL,
    [Title] nvarchar(60) NOT NULL,
    [Text] ntext NOT NULL,
    [CreateDate] datetime NOT NULL,
    [ModifyDate] datetime NULL,
    [ModifyUserId] bigint NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedByUserId] bigint NULL,
    CONSTRAINT PK_Posts PRIMARY KEY CLUSTERED ([PostId] ASC),
    CONSTRAINT FK_Posts_Threads FOREIGN KEY ([ThreadId]) REFERENCES [forums].[Threads] ([ThreadId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_Posts_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_Posts_ModifyUsers FOREIGN KEY ([ModifyUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_Posts_DeletedByUsers FOREIGN KEY ([DeletedByUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
