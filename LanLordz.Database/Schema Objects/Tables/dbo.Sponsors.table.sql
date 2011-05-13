CREATE TABLE [dbo].[Sponsors] (
    [SponsorId] bigint NOT NULL IDENTITY,
    [Name] nvarchar(60) NOT NULL,
    [Info] ntext NULL,
    [Url] nvarchar(MAX) NULL,
    [ImageUrl] nvarchar(MAX) NULL,
    [Order] int NOT NULL,
    CONSTRAINT PK_Sponsors PRIMARY KEY CLUSTERED ([SponsorId] ASC),
    CONSTRAINT IX_Sponsors_Name UNIQUE ([Name] ASC)
)
