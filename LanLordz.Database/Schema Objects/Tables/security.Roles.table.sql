CREATE TABLE [security].[Roles]
(
    [RoleId] bigint NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [IsDefault] bit NOT NULL,
    [IsAdministrator] bit NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED ([RoleId] ASC),
    CONSTRAINT IX_Roles_Name UNIQUE ([Name] ASC)
)
