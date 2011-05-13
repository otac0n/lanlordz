CREATE TABLE [forums].[ForumsAccess]
(
    [ForumId] bigint NOT NULL,
    [AccessType] nvarchar(4) NOT NULL,
    [AccessControlListId] bigint NOT NULL,
    CONSTRAINT PK_ForumsAccess PRIMARY KEY CLUSTERED ([ForumId] ASC, [AccessType] ASC),
    CONSTRAINT FK_ForumsAccess_AccessControlLists FOREIGN KEY ([AccessControlListId]) REFERENCES [security].[AccessControlLists] ([AccessControlListId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_ForumsAccess_Forums FOREIGN KEY ([ForumId]) REFERENCES [forums].[Forums] ([ForumId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
