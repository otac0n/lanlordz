CREATE TABLE [security].[AuthFailures]
(
    [UserId] bigint NOT NULL,
    [OriginatingHostIP] nvarchar(39) NOT NULL,
    [CreateDate] datetime NOT NULL,
    CONSTRAINT PK_AuthFailures PRIMARY KEY CLUSTERED ([UserId] ASC, [OriginatingHostIP] ASC, [CreateDate] ASC),
    CONSTRAINT FK_AuthFailures_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
