# SimpleDb
Library for handling file-based and MSSQL-based databases.

## The SimpleDb
This library contains shared code used by the SimpleDbFiles and the SimpleDbMssql projects.
You have to use this library, if you want to use mentiones libraries.

## The SimpleDbFiles
Backend for the SimpleDb library. Uses files in the file system to store data. Each table
(data object) is stored in a separate directory. Each item of that table is stored in a 
separate file in that directory. Each data object using this library has to have the Id
column, because it is used to generate names for stored items.

This library is best for very simple and small databases.

## The SimpleDbMssql
Backend for the SimpleDb library. Uses MSSQL for storing data. 

This library is best for more complicated stuff.

# Licence

SimpleDb - (C) 2016 Premysl Fara 
 
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
