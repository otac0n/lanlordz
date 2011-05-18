CREATE TABLE [events].[Pairings]
(
    [PairingId] bigint IDENTITY NOT NULL,
    [RoundId] bigint NOT NULL,
    CONSTRAINT PK_Pairings PRIMARY KEY CLUSTERED ([PairingId] ASC),
    CONSTRAINT FK_Pairings_Rounds FOREIGN KEY ([RoundId]) REFERENCES [events].[Rounds] ([RoundId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
