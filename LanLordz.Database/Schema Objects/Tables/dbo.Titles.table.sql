CREATE TABLE [dbo].[Titles]
(
    [TitleId] bigint NOT NULL IDENTITY,
    [UserId] bigint NULL,
    [RoleId] bigint NULL,
    [PostCountThreshold] int NULL,
    [TitleText] nvarchar(50) NOT NULL,
    CONSTRAINT PK_Titles PRIMARY KEY CLUSTERED ([TitleId] ASC),
    CONSTRAINT FK_Titles_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_Titles_Roles FOREIGN KEY ([RoleId]) REFERENCES [security].[Roles] ([RoleId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
