CREATE TABLE [security].[AccessControlLists]
(
    [AccessControlListId] bigint NOT NULL IDENTITY,
    [Name] nvarchar(50) NULL,
    CONSTRAINT PK_AccessControlLists PRIMARY KEY CLUSTERED ([AccessControlListId] ASC),
)
