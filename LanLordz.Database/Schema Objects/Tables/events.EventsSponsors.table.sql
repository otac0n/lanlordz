CREATE TABLE [events].[EventsSponsors]
(
    [EventSponsorId] bigint IDENTITY NOT NULL,
    [EventId] bigint NOT NULL,
    [SponsorId] bigint NOT NULL,
    [ConfirmedDate] datetime NOT NULL,
    CONSTRAINT PK_EventsSponsors PRIMARY KEY CLUSTERED ([EventSponsorId] ASC),
    CONSTRAINT FK_EventsSponsors_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_EventsSponsors_Sponsors FOREIGN KEY ([SponsorId]) REFERENCES [dbo].[Sponsors] ([SponsorId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT IX_EventsSponsors_EventId_SponsorId UNIQUE ([EventId] ASC, [SponsorId] ASC)
)
