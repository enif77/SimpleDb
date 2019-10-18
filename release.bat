@REM Creates nuget packages from all relevant projects.

@SET PATH=%PATH%;W:\Devel\bin\nuget

@REM If this is 0, it is a normal release.
@REM If this is not 0, it is a rerelease. Existing packages with the same versions are deleted from the source first, then readded.
@SET RERELEASE=0

@REM A version of each package.
@SET CORE_VERSION=1.0.1
@SET SQL_VERSION=1.0.3
@SET ELKPS_VERSION=1.0.3
@SET EVALS_VERSION=1.0.1
@SET ISQLT_VERSION=1.0.3
@SET IFRBI_VERSION=1.0.3

@REM Direscories.
@SET BUILD_START_DIR=%CD%
@SET NUGET_DIR=W:\Devel\nuget
@rem @SET NUGET_DIR=C:\Users\enif\source\nuget

@REM --- CLEANUP ---

@IF %RERELEASE%==0 GOTO skip_cleanup

@REM Delete existing packages.
nuget delete SimpleDb.Core %CORE_VERSION% -noninteractive -source %NUGET_DIR%
nuget delete SimpleDb.Sql %SQL_VERSION% -noninteractive -source %NUGET_DIR%
nuget delete SimpleDb.Extensions.Lookups %ELKPS_VERSION% -noninteractive -source %NUGET_DIR%
nuget delete SimpleDb.Extensions.Validations %EVALS_VERSION% -noninteractive -source %NUGET_DIR%
nuget delete SimpleDb.Firebird %ISQLT_VERSION% -noninteractive -source %NUGET_DIR%
nuget delete SimpleDb.SqLite %IFRBI_VERSION% -noninteractive -source %NUGET_DIR%

:skip_cleanup

@REM --- CORE ---

CD SimpleDb.Core

nuget.exe pack
nuget.exe add SimpleDb.Core.%CORE_VERSION%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- SQL ---

CD SimpleDb.Sql

nuget.exe pack
nuget.exe add SimpleDb.Sql.%SQL_VERSION%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- Extensions.Lookups ---

CD SimpleDb.Extensions.Lookups

nuget.exe pack
nuget.exe add SimpleDb.Extensions.Lookups.%ELKPS_VERSION%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- Extensions.Validations ---

CD SimpleDb.Extensions.Validations

nuget.exe pack
nuget.exe add SimpleDb.Extensions.Validations.%EVALS_VERSION%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- Firebird ---

CD SimpleDb.Firebird

nuget.exe pack
nuget.exe add SimpleDb.Firebird.%IFRBI_VERSION%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- SqLite ---

CD SimpleDb.SqLite

nuget.exe pack
nuget.exe add SimpleDb.SqLite.%ISQLT_VERSION%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

