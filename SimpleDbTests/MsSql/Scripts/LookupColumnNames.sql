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

CREATE TABLE [dbo].[LookupColumnNames] (
    [Id]    INT            IDENTITY (0, 1) NOT NULL,
    [RenamedName] NVARCHAR (3)   NOT NULL,
    [Description] NVARCHAR (MAX) DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [Un_LookupColumnNames_RenamedName] UNIQUE NONCLUSTERED ([RenamedName] ASC)
);

GO


CREATE PROC [dbo].[spLookupColumnNames_SelectList]
AS
BEGIN
    SELECT * FROM LookupColumnNames
END

GO


CREATE PROC [dbo].[spLookupColumnNames_SelectDetails] 
(
    @Id	INT
)
AS
BEGIN
    SELECT * FROM LookupColumnNames WHERE Id = @Id
END

GO


CREATE PROC [dbo].[spLookupColumnNames_Insert] 
( 
      @RenamedName NVARCHAR(3)
    , @Description NVARCHAR(max)	= ''
) 
AS
BEGIN
    INSERT INTO LookupColumnNames (RenamedName, Description) VALUES (@RenamedName, @Description)
    SELECT SCOPE_IDENTITY() [Id]
END

GO


CREATE PROC [dbo].[spLookupColumnNames_Delete] 
(
    @Id	INT
)
AS
BEGIN
    DELETE FROM LookupColumnNames WHERE Id = @Id
    SELECT @@ROWCOUNT [RowCount]
END

GO


CREATE FUNCTION [dbo].[fnLookupColumnNames_GetIdByName]
(
    @Name	nvarchar(3)
)
RETURNS	int
AS
BEGIN
    DECLARE	@Id	int
    
    SELECT @Id = [Id] FROM LookupColumnNames WHERE RenamedName = @Name

    RETURN	@Id
END

GO


-- If nothing is selected (The Id = 0), we display/use this item.
INSERT INTO LookupColumnNames (RenamedName, Description) VALUES ('-', '')

-- Normal selectable items.
INSERT INTO LookupColumnNames (RenamedName, Description) VALUES ('R1', 'The first renamed Name column value')
INSERT INTO LookupColumnNames (RenamedName, Description) VALUES ('R2', 'The second renamed Name column value')
INSERT INTO LookupColumnNames (RenamedName, Description) VALUES ('R3', 'The third renamed Name column value')

GO
