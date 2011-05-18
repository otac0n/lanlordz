CREATE TABLE [dbo].[Configuration]
(
    [Property] nvarchar(50) NOT NULL,
    [Value] nvarchar(MAX) NOT NULL,
    CONSTRAINT PK_Configuration PRIMARY KEY CLUSTERED ([Property] ASC)
)
