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

USE [master]
GO

-- Database in a specific location.
CREATE DATABASE [SIMPLEDB]
CONTAINMENT = NONE
ON  PRIMARY 
(NAME = N'SIMPLEDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\SIMPLEDB.mdf', SIZE = 4160KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB)
LOG ON 
(NAME = N'SIMPLEDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\SIMPLEDB_log.ldf', SIZE = 1040KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO

-- Database in a default location.
CREATE DATABASE [SIMPLEDB] 
GO
ALTER DATABASE SIMPLEDB MODIFY FILE (NAME = N'SIMPLEDB' , SIZE = 4160KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB)
GO
ALTER DATABASE SIMPLEDB MODIFY FILE (NAME = N'SIMPLEDB_log' , SIZE = 1040KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO


-- NOTE: Choose either the specific location or the default location variation and comment out the other one.
