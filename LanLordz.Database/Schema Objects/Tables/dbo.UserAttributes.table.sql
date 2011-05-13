CREATE TABLE [dbo].[UserAttributes] (
    [UserAttributeId] bigint NOT NULL IDENTITY,
    [UserId] bigint NOT NULL,
    [Attribute] nvarchar(50) NOT NULL,
    [Value] nvarchar(MAX) NULL,
    CONSTRAINT PK_UserAttributes PRIMARY KEY CLUSTERED ([UserAttributeId] ASC),
    CONSTRAINT IX_UserAttributes_UserId_Attribute UNIQUE ([UserId] ASC, [Attribute] ASC),
    CONSTRAINT FK_UserAttributes_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
