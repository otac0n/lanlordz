CREATE TABLE [events].[Rounds]
(
    [RoundId] bigint IDENTITY NOT NULL,
    [TournamentId] bigint NOT NULL,
    [RoundNumber] int NOT NULL,
    CONSTRAINT PK_Rounds PRIMARY KEY CLUSTERED ([RoundId] ASC),
    CONSTRAINT IX_Rounds_TournamentId_RoundNumber UNIQUE ([TournamentId] ASC, [RoundNumber] ASC),
    CONSTRAINT FK_Rounds_Tournaments FOREIGN KEY ([TournamentId]) REFERENCES [events].[Tournaments] ([TournamentId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
