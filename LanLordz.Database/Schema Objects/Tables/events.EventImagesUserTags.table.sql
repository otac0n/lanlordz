CREATE TABLE [events].[EventImagesUserTags]
(
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
