CREATE TABLE [security].[AccessControlItems]
(
    [AccessControlItemId] bigint NOT NULL IDENTITY,
    [AccessControlListId] bigint NOT NULL,
    [Type] char(1) NOT NULL,
    [For] nvarchar(50) NOT NULL,
    [Allow] bit NOT NULL,
    [Order] int NOT NULL,
    CONSTRAINT PK_AccessControlItems PRIMARY KEY CLUSTERED ([AccessControlItemId] ASC),
    CONSTRAINT FK_AccessControlItems_AccessControlLists FOREIGN KEY ([AccessControlListId]) REFERENCES [security].[AccessControlLists] ([AccessControlListId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
