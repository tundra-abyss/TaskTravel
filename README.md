# TaskTravel

A small proof-of-concept Windows 10 app that provides a centered, rounded, translucent dock similar to the Windows 11 taskbar.

This repository contains the source for v1.0 (no Start button or tray icon). v1.1 will add a Start button and tray icon.

Build (produce a self-contained single-file .exe)
1. Install .NET 6 SDK (or 7; the csproj targets net6.0-windows).
2. From the project folder:
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true -o publish

3. The single-file exe will be in the `publish/` folder (e.g. publish/TaskTravel.exe).

Usage
- Optionally set Windows taskbar to Auto-hide to let this dock be the visible taskbar (Settings > Personalization > Taskbar).
- Run the exe from the publish folder.

License
- MIT (see LICENSE).
