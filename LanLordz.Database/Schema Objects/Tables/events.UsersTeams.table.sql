CREATE TABLE [events].[UsersTeams]
(
    [UserTeamId] bigint IDENTITY NOT NULL,
    [TeamId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [IsTeamCaptain] bit NOT NULL,
    [Rating] int NOT NULL,
    CONSTRAINT PK_UsersTeams PRIMARY KEY CLUSTERED ([UserTeamId] ASC),
    CONSTRAINT FK_UsersTeams_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_UsersTeams_Teams FOREIGN KEY ([TeamId]) REFERENCES [events].[Teams] ([TeamId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
