CREATE TABLE [forums].[ForumGroups]
(
    [ForumGroupId] bigint NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Order] int NOT NULL,
    CONSTRAINT PK_ForumGroups PRIMARY KEY CLUSTERED ([ForumGroupId] ASC)
)
