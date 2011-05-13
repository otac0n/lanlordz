CREATE TABLE [events].[EventImages]
(
    [EventImageId] bigint IDENTITY NOT NULL,
    [EventId] bigint NOT NULL,
    [ScrapeBusterKey] nvarchar(10) NOT NULL,
    [Image] image NULL,
    CONSTRAINT PK_EventImages PRIMARY KEY CLUSTERED ([EventImageId] ASC),
    CONSTRAINT FK_EventImages_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
