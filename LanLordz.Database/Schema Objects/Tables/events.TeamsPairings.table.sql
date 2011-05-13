CREATE TABLE [events].[TeamsPairings]
(
    [TeamPairingId] bigint IDENTITY NOT NULL,
    [TeamId] bigint NOT NULL,
    [PairingId] bigint NOT NULL,
    [Score] nvarchar(50) NULL,
    CONSTRAINT PK_TeamsPairings PRIMARY KEY CLUSTERED ([TeamPairingId] ASC),
    CONSTRAINT IX_TeamsPairings UNIQUE NONCLUSTERED ([PairingId] ASC, [TeamId] ASC),
    CONSTRAINT FK_TeamsPairings_Teams FOREIGN KEY ([TeamId]) REFERENCES [events].[Teams] ([TeamId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_TeamsPairings_Pairings FOREIGN KEY ([PairingId]) REFERENCES [events].[Pairings] ([PairingId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
