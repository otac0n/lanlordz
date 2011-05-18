CREATE TABLE [forums].[Forums]
(
    [ForumId] bigint NOT NULL IDENTITY,
    [ForumGroupId] bigint NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Description] ntext NULL,
    [Order] int NOT NULL,
    CONSTRAINT PK_Forums PRIMARY KEY CLUSTERED ([ForumId] ASC),
    CONSTRAINT FK_Forums_ForumGroups FOREIGN KEY ([ForumGroupId]) REFERENCES [forums].[ForumGroups] ([ForumGroupId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
