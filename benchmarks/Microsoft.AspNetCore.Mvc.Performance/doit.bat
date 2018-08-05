setlocal

rem Redirect away from %USER%\.nuget\packages to avoid picking up a stale package.
set NUGET_PACKAGES=c:\packages-cache

dotnet publish -c Release -r win-x64 -f netcoreapp2.1 || exit /b 1
dotnet publish -c Release -r win-x64 -f netcoreapp2.2 || exit /b 1

pause
%~dp0bin\Release\netcoreapp2.2\win-x64\publish\Microsoft.AspNetCore.Mvc.Performance.exe --config profile 1
