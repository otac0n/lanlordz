CREATE TABLE [forums].[ThreadLevels]
(
    [Level] bigint NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    CONSTRAINT PK_ThreadLevels PRIMARY KEY CLUSTERED ([Level] ASC)
)
