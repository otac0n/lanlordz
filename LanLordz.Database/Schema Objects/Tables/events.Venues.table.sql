CREATE TABLE [events].[Venues]
(
    [VenueId] bigint IDENTITY NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Address] nvarchar(500) NULL,
    [Latitude] decimal(8,5) NULL,
    [Longitude] decimal(8,5) NULL,
    CONSTRAINT PK_Venues PRIMARY KEY CLUSTERED ([VenueId] ASC)
)
