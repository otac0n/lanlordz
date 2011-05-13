CREATE TABLE [events].[Prizes]
(
    [PrizeId] bigint IDENTITY NOT NULL,
    [EventId] bigint NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [WinnerUserId] bigint NULL,
    CONSTRAINT PK_Prizes PRIMARY KEY CLUSTERED ([PrizeId] ASC),
    CONSTRAINT FK_Prizes_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_Prizes_WinnerUsers FOREIGN KEY ([WinnerUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
