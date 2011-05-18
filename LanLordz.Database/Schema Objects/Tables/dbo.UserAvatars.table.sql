CREATE TABLE [dbo].[UserAvatars] (
    [UserId] bigint NOT NULL,
    [Avatar] image NULL,
    CONSTRAINT PK_UserAvatars PRIMARY KEY CLUSTERED ([UserId] ASC),
    CONSTRAINT FK_UserAvatars_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
