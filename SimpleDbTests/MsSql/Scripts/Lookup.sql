/* SimpleDbTests - (C) 2016 - 2017 Premysl Fara 
 
SimpleDbTests is available under the zlib license:

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
   claim that you wrote the original software. If you use this software
   in a product, an acknowledgment in the product documentation would be
   appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
   misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
 
 */

CREATE TABLE [dbo].[Lookup] (
    [Id]    INT            IDENTITY (0, 1) NOT NULL,
    [Name] NVARCHAR (3)   NOT NULL,
    [Description] NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Un_Lookup_Name] UNIQUE NONCLUSTERED ([Name] ASC)
);

GO


CREATE PROC [dbo].[spLookup_SelectList]
AS
BEGIN
    SELECT * FROM Lookup
END

GO


CREATE PROC [dbo].[spLookup_SelectDetails] 
(
    @Id	INT
)
AS
BEGIN
    SELECT * FROM Lookup WHERE Id = @Id
END

GO


CREATE PROC [dbo].[spLookup_Insert] 
( 
      @Name NVARCHAR(3)
    , @Description NVARCHAR(max)	= ''
) 
AS
BEGIN
    INSERT INTO Lookup (Name, Description) VALUES (@Name, @Description)

    SELECT SCOPE_IDENTITY() [Id]
END

GO


CREATE PROC [dbo].[spLookup_Delete] 
(
    @Id	INT
)
AS
BEGIN
    DELETE FROM Lookup WHERE Id = @Id

    SELECT @@ROWCOUNT [RowCount]
END

GO


CREATE PROC [dbo].[spLookup_GetIdByName]
(
    @Name	nvarchar(3)
)
AS
BEGIN
    SELECT [Id] FROM Lookup WHERE Name = @Name
END

GO


CREATE FUNCTION [dbo].[fnLookup_GetIdByName]
(
    @Name	nvarchar(3)
)
RETURNS	int
AS
BEGIN
    DECLARE	@Id	int
    
    SELECT @Id = [Id] FROM Lookup WHERE Name = @Name

    RETURN	@Id
END

GO


CREATE PROCEDURE [dbo].[spLookup_DeleteAll]
AS
BEGIN
    SET NOCOUNT ON

    TRUNCATE TABLE	[dbo].[Lookup]

    DBCC CHECKIDENT(Lookup, RESEED, 0) WITH NO_INFOMSGS
    
END

GO



-- If nothing is selected (The Id = 0), we display/use this item.
INSERT INTO Lookup (Name, Description) VALUES ('-', '')

-- Normal selectable items.
INSERT INTO Lookup (Name, Description) VALUES ('V1', 'The first value')
INSERT INTO Lookup (Name, Description) VALUES ('V2', 'The second value')
INSERT INTO Lookup (Name, Description) VALUES ('V3', 'The third value')

GO
