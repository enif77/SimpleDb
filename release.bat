@REM Creates nuget packages from all relevant projects.

@SET PATH=%PATH%;W:\Devel\bin\nuget

@REM If this is 0, it is a normal release.
@REM If this is not 0, it is a rerelease. Existing packages with the same versions are deleted from the source first, then readded.
@SET RERELEASE=1

@REM A version of each package.
@SET CORE_VERSION=1.0.3
@SET SQL_VERSION=1.0.3
@SET ELKPS_VERSION=1.0.3
@SET EVALS_VERSION=1.0.3
@SET ISQLT_VERSION=1.0.3
@SET IFRBI_VERSION=1.0.3

@REM A Debug or release version?
rem @SET BUILD_CONFIGURATION=Release
@SET BUILD_CONFIGURATION=Debug
rem @SET WITH_SYSMBOLS=
@SET WITH_SYSMBOLS=.symbols

@REM Direscories.
@SET BUILD_START_DIR=%CD%
@SET NUGET_DIR=W:\Devel\nuget
@rem @SET NUGET_DIR=C:\Users\enif\source\nuget

@REM .NET parameters
@REM https://docs.microsoft.com/cs-cz/dotnet/core/tools/dotnet-pack
@REM https://docs.microsoft.com/cs-cz/nuget/reference/msbuild-targets#pack-target
@REM https://docs.microsoft.com/cs-cz/nuget/quickstart/create-and-publish-a-package-using-the-dotnet-cli
@SET DOTNET_PACK_PARAMS=--configuration %BUILD_CONFIGURATION% --force --include-source --output nupkgs --verbosity minimal

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

dotnet pack %DOTNET_PACK_PARAMS%
nuget.exe add nupkgs\SimpleDb.Core.%CORE_VERSION%%WITH_SYSMBOLS%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- SQL ---

CD SimpleDb.Sql

dotnet pack %DOTNET_PACK_PARAMS%
nuget.exe add nupkgs\SimpleDb.Sql.%SQL_VERSION%%WITH_SYSMBOLS%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- Extensions.Lookups ---

CD SimpleDb.Extensions.Lookups

dotnet pack %DOTNET_PACK_PARAMS%
nuget.exe add nupkgs\SimpleDb.Extensions.Lookups.%ELKPS_VERSION%%WITH_SYSMBOLS%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- Extensions.Validations ---

CD SimpleDb.Extensions.Validations

dotnet pack %DOTNET_PACK_PARAMS%
nuget.exe add nupkgs\SimpleDb.Extensions.Validations.%EVALS_VERSION%%WITH_SYSMBOLS%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- Firebird ---

CD SimpleDb.Firebird

dotnet pack %DOTNET_PACK_PARAMS%
nuget.exe add nupkgs\SimpleDb.Firebird.%IFRBI_VERSION%%WITH_SYSMBOLS%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

@REM --- SqLite ---

CD SimpleDb.SqLite

dotnet pack %DOTNET_PACK_PARAMS%
nuget.exe add nupkgs\SimpleDb.SqLite.%ISQLT_VERSION%%WITH_SYSMBOLS%.nupkg -source %NUGET_DIR%

CD %BUILD_START_DIR%

