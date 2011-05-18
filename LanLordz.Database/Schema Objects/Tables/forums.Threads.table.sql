CREATE TABLE [forums].[Threads]
(
    [ThreadId] bigint NOT NULL IDENTITY,
    [ForumId] bigint NOT NULL,
    [Title] nvarchar(60) NOT NULL,
    [CreateDate] datetime NOT NULL,
    [Level] bigint NOT NULL,
    [Views] bigint NOT NULL,
    [IsLocked] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedByUserId] bigint NULL,
    CONSTRAINT PK_Threads PRIMARY KEY CLUSTERED ([ThreadId] ASC),
    CONSTRAINT FK_Threads_Forums FOREIGN KEY ([ForumId]) REFERENCES [forums].[Forums] ([ForumId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_Threads_ThreadLevels FOREIGN KEY ([Level]) REFERENCES [forums].[ThreadLevels] ([Level]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
