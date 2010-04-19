CREATE SCHEMA [forums] AUTHORIZATION dbo
GO

CREATE SCHEMA [events] AUTHORIZATION dbo
GO

CREATE SCHEMA [security] AUTHORIZATION dbo
GO

CREATE TABLE [dbo].[Configuration] (
	[Property] nvarchar(50) NOT NULL,
	[Value] nvarchar(MAX) NOT NULL,
	CONSTRAINT PK_Configuration PRIMARY KEY CLUSTERED ([Property] ASC)
)
GO

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
GO

CREATE TABLE [dbo].[Users] (
	[UserId] bigint NOT NULL IDENTITY,
	[Username] nvarchar(50) NOT NULL,
	[PasswordHash] char(88) NOT NULL,
	[SecurityQuestion] nvarchar(100) NOT NULL,
	[SecurityAnswer] nvarchar(50) NOT NULL,
	[CreateDate] datetime NOT NULL,
	[Email] nvarchar(50) NOT NULL,
	[ShowEmail] bit NOT NULL,
	[ReceiveAdminEmail] bit NOT NULL,
	[Gender] char(1) NOT NULL,
	[ShowGender] bit NOT NULL,
	[IsEmailConfirmed] bit NOT NULL,
	CONSTRAINT PK_Users PRIMARY KEY CLUSTERED ([UserId] ASC),
	CONSTRAINT IX_Users_Username UNIQUE ([Username] ASC),
	CONSTRAINT IX_Users_Email UNIQUE ([Email] ASC)
)
GO

CREATE TABLE [dbo].[UserAvatars] (
	[UserId] bigint NOT NULL,
	[Avatar] image NULL,
	CONSTRAINT PK_UserAvatars PRIMARY KEY CLUSTERED ([UserId] ASC),
	CONSTRAINT FK_UserAvatars_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [dbo].[UserAttributes] (
    [UserAttributeId] bigint NOT NULL IDENTITY,
	[UserId] bigint NOT NULL,
	[Attribute] nvarchar(50) NOT NULL,
	[Value] nvarchar(MAX) NULL,
	CONSTRAINT PK_UserAttributes PRIMARY KEY CLUSTERED ([UserAttributeId] ASC),
	CONSTRAINT IX_UserAttributes_UserId_Attribute UNIQUE ([UserId] ASC, [Attribute] ASC),
	CONSTRAINT FK_UserAttributes_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [dbo].[EmailConfirm]
(
	[Email] nvarchar(50) NOT NULL,
	[Key] uniqueidentifier NOT NULL,
	CONSTRAINT PK_EmailConfirm PRIMARY KEY CLUSTERED ([Email] ASC)
)
GO

CREATE TABLE [dbo].[Polls]
(
	[PollId] bigint NOT NULL IDENTITY,
	[CreatorUserId] bigint NOT NULL,
	[Title] nvarchar(60) NOT NULL,
	[IsPrivate] bit NOT NULL,
	[IsMultiAnswer] bit NOT NULL,
	[Text] ntext NOT NULL,
	CONSTRAINT PK_Polls PRIMARY KEY CLUSTERED ([PollId] ASC),
	CONSTRAINT FK_Polls_CreatorUsers FOREIGN KEY ([CreatorUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [dbo].[PollResponses](
	[PollResponseId] bigint IDENTITY NOT NULL,
	[PollId] bigint NOT NULL,
	[Label] nvarchar(60) NOT NULL,
	CONSTRAINT PK_PollResponses PRIMARY KEY CLUSTERED ([PollResponseId] ASC),
	CONSTRAINT FK_PollResponses_Polls FOREIGN KEY ([PollId]) REFERENCES [dbo].[Polls] ([PollId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
)
GO

CREATE TABLE [dbo].[UsersPollResponses](
	[UserPollResponseId] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[PollResponseId] [bigint] NOT NULL,
	[IsSelected] [bit] NOT NULL,
	CONSTRAINT PK_UsersPollResponses PRIMARY KEY CLUSTERED ([UserPollResponseId] ASC),
	CONSTRAINT FK_UsersPollResponses_PollResponses FOREIGN KEY ([PollResponseId]) REFERENCES [dbo].[PollResponses] ([PollResponseId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_UsersPollResponses_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT IX_UsersPollResponses_UserId_PollResponseId UNIQUE ([UserId] ASC, [PollResponseId] ASC)
)
GO

CREATE TABLE [security].[Roles] (
	[RoleId] bigint NOT NULL IDENTITY,
	[Name] nvarchar(50) NOT NULL,
	[IsDefault] bit NOT NULL,
	[IsAdministrator] bit NOT NULL,
	CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED ([RoleId] ASC),
	CONSTRAINT IX_Roles_Name UNIQUE ([Name] ASC)
)
GO

CREATE TABLE [security].[UsersRoles] (
    [UserRoleId] bigint NOT NULL IDENTITY,
	[UserId] bigint NOT NULL,
	[RoleId] bigint NOT NULL,
	CONSTRAINT PK_UsersRoles PRIMARY KEY CLUSTERED ([UserRoleId] ASC),
	CONSTRAINT IX_UsersRoles_UserId_RoleId UNIQUE ([UserId] ASC, [RoleId] ASC),
	CONSTRAINT FK_UsersRoles_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_UsersRoles_Roles FOREIGN KEY ([RoleId]) REFERENCES [security].[Roles] ([RoleId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [dbo].[Titles] (
	[TitleId] bigint NOT NULL IDENTITY,
	[UserId] bigint NULL,
	[RoleId] bigint NULL,
	[PostCountThreshold] int NULL,
	[TitleText] nvarchar(50) NOT NULL,
	CONSTRAINT PK_Titles PRIMARY KEY CLUSTERED ([TitleId] ASC),
	CONSTRAINT FK_Titles_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_Titles_Roles FOREIGN KEY ([RoleId]) REFERENCES [security].[Roles] ([RoleId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [security].[AuthFailures] (
    [UserId] bigint NOT NULL,
    [OriginatingHostIP] nvarchar(39) NOT NULL,
	[CreateDate] datetime NOT NULL,
	CONSTRAINT PK_AuthFailures PRIMARY KEY CLUSTERED ([UserId] ASC, [OriginatingHostIP] ASC, [CreateDate] ASC),
	CONSTRAINT FK_AuthFailures_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [security].[AutoLogin] (
	[Key] uniqueidentifier NOT NULL,
	[UserId] bigint NOT NULL,
	[ExpirationDate] datetime NOT NULL,
	CONSTRAINT PK_AutoLogin PRIMARY KEY CLUSTERED ([Key] ASC),
	CONSTRAINT FK_AutoLogin_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [security].[SecurityKeys] (
	[UserId] bigint NOT NULL,
	[Key] uniqueidentifier NOT NULL,
	[ExpirationDate] datetime NOT NULL,
	CONSTRAINT PK_SecurityKeys PRIMARY KEY CLUSTERED ([UserId] ASC),
	CONSTRAINT FK_SecurityKeys_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION 
)

CREATE TABLE [security].[AccessControlLists] (
	[AccessControlListId] bigint NOT NULL IDENTITY,
	[Name] nvarchar(50) NULL,
	CONSTRAINT PK_AccessControlLists PRIMARY KEY CLUSTERED ([AccessControlListId] ASC),
)
GO

CREATE TABLE [security].[AccessControlItems] (
	[AccessControlItemId] bigint NOT NULL IDENTITY,
	[AccessControlListId] bigint NOT NULL,
	[Type] nvarchar(1) NOT NULL,
	[For] nvarchar(50) NOT NULL,
	[Allow] bit NOT NULL,
	[Order] int NOT NULL,
	CONSTRAINT PK_AccessControlItems PRIMARY KEY CLUSTERED ([AccessControlItemId] ASC),
	CONSTRAINT FK_AccessControlItems_AccessControlLists FOREIGN KEY ([AccessControlListId]) REFERENCES [security].[AccessControlLists] ([AccessControlListId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [forums].[ThreadLevels] (
	[Level] bigint NOT NULL,
	[Name] nvarchar(50) NOT NULL,
	CONSTRAINT PK_ThreadLevels PRIMARY KEY CLUSTERED ([Level] ASC)
)
GO

CREATE TABLE [forums].[ForumGroups] (
	[ForumGroupId] bigint NOT NULL IDENTITY,
	[Name] nvarchar(50) NOT NULL,
	[Order] int NOT NULL,
	CONSTRAINT PK_ForumGroups PRIMARY KEY CLUSTERED ([ForumGroupId] ASC)
)
GO

CREATE TABLE [forums].[Forums] (
	[ForumId] bigint NOT NULL IDENTITY,
	[ForumGroupId] bigint NOT NULL,
	[Name] nvarchar(50) NOT NULL,
	[Description] ntext NULL,
	[Order] int NOT NULL,
	CONSTRAINT PK_Forums PRIMARY KEY CLUSTERED ([ForumId] ASC),
	CONSTRAINT FK_Forums_ForumGroups FOREIGN KEY ([ForumGroupId]) REFERENCES [forums].[ForumGroups] ([ForumGroupId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [forums].[Threads] (
	[ThreadId] bigint NOT NULL IDENTITY,
	[ForumId] bigint NOT NULL,
	[Title] nvarchar(60) NOT NULL,
	[CreateDate] datetime NOT NULL,
	[Level] bigint NOT NULL,
	[Views] bigint NOT NULL,
	[IsLocked] bit NOT NULL,
	[IsDeleted] bit NOT NULL,
	[DeletedByUserId] bigint NULL,
	CONSTRAINT PK_Threads PRIMARY KEY CLUSTERED ([ThreadId] ASC),
	CONSTRAINT FK_Threads_Forums FOREIGN KEY ([ForumId]) REFERENCES [forums].[Forums] ([ForumId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_Threads_ThreadLevels FOREIGN KEY ([Level]) REFERENCES [forums].[ThreadLevels] ([Level]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [forums].[Posts] (
	[PostId] bigint NOT NULL IDENTITY,
	[ThreadId] bigint NOT NULL,
	[ResponseToPostId] bigint NULL,
	[UserId] bigint NOT NULL,
	[Title] nvarchar(60) NOT NULL,
	[Text] ntext NOT NULL,
	[CreateDate] datetime NOT NULL,
	[ModifyDate] datetime NULL,
	[ModifyUserId] bigint NULL,
	[IsDeleted] bit NOT NULL,
	[DeletedByUserId] bigint NULL,
	CONSTRAINT PK_Posts PRIMARY KEY CLUSTERED ([PostId] ASC),
	CONSTRAINT FK_Posts_Threads FOREIGN KEY ([ThreadId]) REFERENCES [forums].[Threads] ([ThreadId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_Posts_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_Posts_ModifyUsers FOREIGN KEY ([ModifyUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_Posts_DeletedByUsers FOREIGN KEY ([DeletedByUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE NONCLUSTERED INDEX IX_Posts_UserID ON [forums].[Posts] ([UserId])
CREATE NONCLUSTERED INDEX IX_Posts_ThreadID ON [forums].[Posts] ([ThreadId])
GO

CREATE TABLE [forums].[ThreadRead] (
    [ThreadReadId] bigint NOT NULL IDENTITY,
	[ThreadId] bigint NOT NULL,
	[UserId] bigint NOT NULL,
	[DateRead] datetime NOT NULL,
	CONSTRAINT PK_ThreadRead PRIMARY KEY CLUSTERED ([ThreadReadId] ASC),
	CONSTRAINT IX_ThreadRead_ThreadId_UserId UNIQUE ([ThreadId] DESC, [UserId] ASC),
	CONSTRAINT FK_ThreadRead_Threads FOREIGN KEY ([ThreadId]) REFERENCES [forums].[Threads] ([ThreadId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_ThreadRead_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [forums].[ForumGroupsAccess] (
	[ForumGroupId] bigint NOT NULL,
	[AccessType] nvarchar(4) NOT NULL,
	[AccessControlListId] bigint NOT NULL,
	CONSTRAINT PK_ForumGroupsAccess PRIMARY KEY CLUSTERED ([ForumGroupId] ASC, [AccessType] ASC),
	CONSTRAINT FK_ForumGroupsAccess_AccessControlLists FOREIGN KEY ([AccessControlListId]) REFERENCES [security].[AccessControlLists] ([AccessControlListId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_ForumGroupsAccess_ForumGroups FOREIGN KEY ([ForumGroupId]) REFERENCES [forums].[ForumGroups] ([ForumGroupId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [forums].[ForumsAccess] (
	[ForumId] bigint NOT NULL,
	[AccessType] nvarchar(4) NOT NULL,
	[AccessControlListId] bigint NOT NULL,
	CONSTRAINT PK_ForumsAccess PRIMARY KEY CLUSTERED ([ForumId] ASC, [AccessType] ASC),
	CONSTRAINT FK_ForumsAccess_AccessControlLists FOREIGN KEY ([AccessControlListId]) REFERENCES [security].[AccessControlLists] ([AccessControlListId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_ForumsAccess_Forums FOREIGN KEY ([ForumId]) REFERENCES [forums].[Forums] ([ForumId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [events].[Venues] (
	[VenueId] bigint IDENTITY NOT NULL,
	[Name] nvarchar(150) NOT NULL,
	[Address] nvarchar(500) NULL,
	[Latitude] decimal(8,5) NULL,
	[Longitude] decimal(8,5) NULL,
	CONSTRAINT PK_Venues PRIMARY KEY CLUSTERED ([VenueId] ASC)
)
GO

CREATE TABLE [events].[Events] (
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
GO

CREATE TABLE [events].[Registrations] (
	[EventId] bigint NOT NULL,
	[UserId] bigint NOT NULL,
	[RegistrationDate] datetime NOT NULL,
	[IsCheckedIn] bit NOT NULL,
	CONSTRAINT PK_Registrations PRIMARY KEY CLUSTERED ([EventId] ASC, [UserId] ASC),
	CONSTRAINT FK_Registrations_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_Registrations_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT IX_Registrations_EventId_UserId UNIQUE ([EventId] ASC, [UserId] ASC)
)
GO

CREATE TABLE [events].[EventsSponsors] (
    [EventSponsorId] bigint IDENTITY NOT NULL,
	[EventId] bigint NOT NULL,
	[SponsorId] bigint NOT NULL,
	[ConfirmedDate] datetime NOT NULL,
	CONSTRAINT PK_EventsSponsors PRIMARY KEY CLUSTERED ([EventSponsorId] ASC),
	CONSTRAINT FK_EventsSponsors_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_EventsSponsors_Sponsors FOREIGN KEY ([SponsorId]) REFERENCES [dbo].[Sponsors] ([SponsorId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT IX_EventsSponsors_EventId_SponsorId UNIQUE ([EventId] ASC, [SponsorId] ASC)
)
GO

CREATE TABLE [events].[Prizes] (
    [PrizeId] bigint IDENTITY NOT NULL,
    [EventId] bigint NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [WinnerUserId] bigint NULL,
	CONSTRAINT PK_Prizes PRIMARY KEY CLUSTERED ([PrizeId] ASC),
	CONSTRAINT FK_Prizes_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_Prizes_WinnerUsers FOREIGN KEY ([WinnerUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)

CREATE TABLE [events].[EventImages] (
	[EventImageId] bigint IDENTITY NOT NULL,
	[EventId] bigint NOT NULL,
	[ScrapeBusterKey] nvarchar(10) NOT NULL,
	[Image] image NULL,
	CONSTRAINT PK_EventImages PRIMARY KEY CLUSTERED ([EventImageId] ASC),
	CONSTRAINT FK_EventImages_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [events].[EventImagesUserTags] (
    [EventImageUserTagId] bigint IDENTITY NOT NULL,
    [EventImageId] bigint NOT NULL,
    [TaggedUserUserId] bigint NOT NULL,
    [CreatorUserId] bigint NOT NULL,
    [Region] varbinary(200) NOT NULL,
	CONSTRAINT PK_EventImagesUserTags PRIMARY KEY CLUSTERED ([EventImageUserTagId] ASC),
	CONSTRAINT FK_EventImagesUserTags_EventImage FOREIGN KEY ([EventImageId]) REFERENCES [events].[EventImages] ([EventImageId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_EventImagesUserTags_TaggedUser FOREIGN KEY ([TaggedUserUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT IX_EventImagesUserTags_EventImageId_CreatorUserId_TaggedUserUserId UNIQUE ([EventImageId] ASC, [CreatorUserId] ASC, [TaggedUserUserId] ASC),
	CONSTRAINT FK_EventImagesUserTags_Creator FOREIGN KEY ([CreatorUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [events].[Tournaments] (
    [TournamentId] bigint IDENTITY NOT NULL,
	[EventId] bigint NOT NULL,
    [TournamentDirectorUserId] bigint NULL,
	[Title] nvarchar(150) NOT NULL,
	[Game] nvarchar(150) NULL,
	[ScoreMode] nvarchar(50) NOT NULL,
	[PairingsGenerator] nvarchar(30) NOT NULL,
	[TeamSize] int NOT NULL,
	[AllowLateEntry] bit NOT NULL,
	[IsLocked] bit NOT NULL,
	[GameInfo] ntext NULL,
	[ServerSettings] ntext NULL,
	CONSTRAINT PK_Tournaments PRIMARY KEY CLUSTERED ([TournamentId] ASC),
	CONSTRAINT FK_Tournaments_Events FOREIGN KEY ([EventId]) REFERENCES [events].[Events] ([EventId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
	CONSTRAINT FK_Tournaments_Users FOREIGN KEY ([TournamentDirectorUserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [events].[Teams] (
    [TeamId] bigint IDENTITY NOT NULL,
    [TournamentId] bigint NOT NULL,
    [TeamName] nvarchar(50) NOT NULL,
    [TeamTagFormat] nvarchar(50) NOT NULL,
	CONSTRAINT PK_Teams PRIMARY KEY CLUSTERED ([TeamId] ASC),
	CONSTRAINT FK_Teams_Tournaments FOREIGN KEY ([TournamentId]) REFERENCES [events].[Tournaments] ([TournamentId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [events].[UsersTeams] (
	[UserTeamId] bigint IDENTITY NOT NULL,
    [TeamId] bigint NOT NULL,
    [UserId] bigint NOT NULL,
    [IsTeamCaptain] bit NOT NULL,
    [Rating] int NOT NULL,
    CONSTRAINT PK_UsersTeams PRIMARY KEY CLUSTERED ([UserTeamId] ASC),
    CONSTRAINT FK_UsersTeams_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_UsersTeams_Teams FOREIGN KEY ([TeamId]) REFERENCES [events].[Teams] ([TeamId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [events].[Rounds] (
    [RoundId] bigint IDENTITY NOT NULL,
    [TournamentId] bigint NOT NULL,
    [RoundNumber] int NOT NULL,
    CONSTRAINT PK_Rounds PRIMARY KEY CLUSTERED ([RoundId] ASC),
    CONSTRAINT IX_Rounds_TournamentId_RoundNumber UNIQUE ([TournamentId] ASC, [RoundNumber] ASC),
    CONSTRAINT FK_Rounds_Tournaments FOREIGN KEY ([TournamentId]) REFERENCES [events].[Tournaments] ([TournamentId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [events].[Pairings] (
    [PairingId] bigint IDENTITY NOT NULL,
    [RoundId] bigint NOT NULL,
    CONSTRAINT PK_Pairings PRIMARY KEY CLUSTERED ([PairingId] ASC),
    CONSTRAINT FK_Pairings_Rounds FOREIGN KEY ([RoundId]) REFERENCES [events].[Rounds] ([RoundId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

CREATE TABLE [events].[TeamsPairings] (
    [TeamPairingId] bigint IDENTITY NOT NULL,
    [TeamId] bigint NOT NULL,
    [PairingId] bigint NOT NULL,
    [Score] nvarchar(50) NULL,
    CONSTRAINT PK_TeamsPairings PRIMARY KEY CLUSTERED ([TeamPairingId] ASC),
    CONSTRAINT IX_TeamsPairings UNIQUE NONCLUSTERED ([PairingId] ASC, [TeamId] ASC),
    CONSTRAINT FK_TeamsPairings_Teams FOREIGN KEY ([TeamId]) REFERENCES [events].[Teams] ([TeamId]) ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT FK_TeamsPairings_Pairings FOREIGN KEY ([PairingId]) REFERENCES [events].[Pairings] ([PairingId]) ON UPDATE NO ACTION ON DELETE NO ACTION
)
GO

INSERT INTO [security].[Roles] ([Name], [IsDefault], [IsAdministrator]) VALUES ('Administrator', 0, 1)
INSERT INTO [security].[Roles] ([Name], [IsDefault], [IsAdministrator]) VALUES ('User', 1, 0)
INSERT INTO [security].[Roles] ([Name], [IsDefault], [IsAdministrator]) VALUES ('Crew', 0, 0)
INSERT INTO [security].[Roles] ([Name], [IsDefault], [IsAdministrator]) VALUES ('Banned', 0, 0)
GO