# SimpleDb

Library for handling relational databases. Works with two modes - calls stored procedures
for CRUD operations, or generates SQL queries. Some implementations support both modes,
some just queries (like SqLite).

Supports:

  - SqLite
  - Firebird
  - MSSQL
  - MySQL
  - PostgreSQL (PgSql)
  - Files (very limited "database" :-))

## SimpleDb.Core

This library contains the core functionality used by the rest of this project. Defines data
entities, data attributes and contains the EntityReflector class, that helps with getting
informations about data entities.

## SimpleDb.Sql

Defines all common all SQL stuff. Data layers, data consumers, query generators and WHERE
clause expressions. It is the base for all user data manipulation code.

## SimpleDb.Extensions.Lookups

Extends the **SimpleDb.Sql** project to simplyfy creation od lookup datalayers. A lookup
entity has an Id, Name and Description.

## SimpleDb.Extensions.Validations

Helps with validation of data entities.

## Implementations

There are many different database engines. I created support libraries for some of those.
Here they are sorted by how much I am using them.

### SimpleDb.Sqlite

SqLite version 3. Great for build-in databases. Supports queries only.

### SimpleDb.Firebird

Firebird version 3.x database. Can be used with a server or as an embedded database.

### SimpleDb.*Sql

Various implementations. Working, but need more attention from me... :-)

### SimpleDb.Files

Backend for the SimpleDb library. Uses files in the file system to store data. Each table
(data object) is stored in a separate directory. Each item of that table is stored in a 
separate file in that directory. Each data object using this library has to have the Id
column, because it is used to generate names for stored items.

This library is best for very simple and small databases.

# Licence

SimpleDb - (C) 2016 - 2019 Premysl Fara 
 
SimpleDb is available under the zlib license:

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
