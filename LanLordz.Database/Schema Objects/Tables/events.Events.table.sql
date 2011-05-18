CREATE TABLE [events].[Events]
(
    [EventId] bigint IDENTITY NOT NULL,
    [VenueId] bigint NOT NULL,
    [Title] nvarchar(150) NOT NULL,
    [Info] ntext NULL,
    [BeginDateTime] datetime NOT NULL,
    [EndDateTime] datetime NOT NULL,
    [Seats] bigint NOT NULL,
    CONSTRAINT PK_Events PRIMARY KEY CLUSTERED ([EventId] ASC),
    CONSTRAINT FK_Events_Venues FOREIGN KEY ([VenueId]) REFERENCES [events].[Venues] ([VenueId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
)
