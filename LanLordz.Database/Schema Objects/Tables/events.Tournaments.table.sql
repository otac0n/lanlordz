CREATE TABLE [events].[Tournaments]
(
    [TournamentId] bigint IDENTITY NOT NULL,
    [EventId] bigint NOT NULL,
    [TournamentDirectorUserId] bigint NULL,
    [Title] nvarchar(150) NOT NULL,
    [Game] nvarchar(150) NULL,
    [ScoreMode] nvarchar(50) NOT NULL,
    [PairingsGenerator] nvarchar(30) NOT NULL,
    [TeamSize] int NOT NULL,
    [AllowLateEntry] bit NOT NULL,
    [IsLocked] bit NOT NULL,
    [GameInfo] ntext NULL,
    [ServerSettings] ntext NULL,
    CONSTRAINT PK_Tournaments PRIMARY KEY CLUSTERED ([TournamentId] ASC),
    CONSTRAINT FK_Tournaments_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_Tournaments_Users FOREIGN KEY ([TournamentDirectorUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
