CREATE TABLE [forums].[ForumGroupsAccess]
(
    [ForumGroupId] bigint NOT NULL,
    [AccessType] nvarchar(4) NOT NULL,
    [AccessControlListId] bigint NOT NULL,
    CONSTRAINT PK_ForumGroupsAccess PRIMARY KEY CLUSTERED ([ForumGroupId] ASC, [AccessType] ASC),
    CONSTRAINT FK_ForumGroupsAccess_AccessControlLists FOREIGN KEY ([AccessControlListId]) REFERENCES [security].[AccessControlLists] ([AccessControlListId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_ForumGroupsAccess_ForumGroups FOREIGN KEY ([ForumGroupId]) REFERENCES [forums].[ForumGroups] ([ForumGroupId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
