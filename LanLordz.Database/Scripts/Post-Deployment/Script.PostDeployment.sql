IF NOT EXISTS (SELECT [RoleId] FROM [security].[Roles]) BEGIN
    INSERT INTO [security].[Roles] ([Name], [IsDefault], [IsAdministrator]) VALUES ('Administrator', 0, 1)
    INSERT INTO [security].[Roles] ([Name], [IsDefault], [IsAdministrator]) VALUES ('User', 1, 0)
    INSERT INTO [security].[Roles] ([Name], [IsDefault], [IsAdministrator]) VALUES ('Crew', 0, 0)
    INSERT INTO [security].[Roles] ([Name], [IsDefault], [IsAdministrator]) VALUES ('Banned', 0, 0)
END
