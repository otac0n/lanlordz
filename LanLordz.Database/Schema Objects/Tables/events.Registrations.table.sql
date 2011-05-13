CREATE TABLE [events].[Registrations]
(
    [EventId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [RegistrationDate] datetime NOT NULL,
    [IsCheckedIn] bit NOT NULL,
    CONSTRAINT PK_Registrations PRIMARY KEY CLUSTERED ([EventId] ASC, [UserId] ASC),
    CONSTRAINT FK_Registrations_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_Registrations_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT IX_Registrations_EventId_UserId UNIQUE ([EventId] ASC, [UserId] ASC)
)
