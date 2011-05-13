CREATE TABLE [events].[Teams]
(
    [TeamId] bigint IDENTITY NOT NULL,
    [TournamentId] bigint NOT NULL,
    [TeamName] nvarchar(50) NOT NULL,
    [TeamTagFormat] nvarchar(50) NOT NULL,
    CONSTRAINT PK_Teams PRIMARY KEY CLUSTERED ([TeamId] ASC),
    CONSTRAINT FK_Teams_Tournaments FOREIGN KEY ([TournamentId]) REFERENCES [events].[Tournaments] ([TournamentId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
