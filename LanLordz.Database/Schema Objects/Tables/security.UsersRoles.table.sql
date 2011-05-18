CREATE TABLE [security].[UsersRoles]
(
    [UserRoleId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [RoleId] bigint NOT NULL,
    CONSTRAINT PK_UsersRoles PRIMARY KEY CLUSTERED ([UserRoleId] ASC),
    CONSTRAINT IX_UsersRoles_UserId_RoleId UNIQUE ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT FK_UsersRoles_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_UsersRoles_Roles FOREIGN KEY ([RoleId]) REFERENCES [security].[Roles] ([RoleId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
