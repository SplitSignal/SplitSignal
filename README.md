# Reggie’s Web Companion — Winter Maze

This ASP.NET Core Razor Pages app lives in the repository's `web/` folder and is intended to cover Reggie's web-player portion of SplitSignal. The Unity project and server placeholder remain separate team components.

## Requirements

- .NET 10 SDK
- Visual Studio Community (optional; the command line is sufficient)

## Run locally

From the repository root, run:

```powershell
cd web
dotnet restore
dotnet run
```

Open the local URL printed by `dotnet run`. In Visual Studio, open `web/CollaborativeEscapeRoom.web.csproj`.

## Try the demo

Demo mode is enabled in `appsettings.json` by default. Enter any session code and player name to open the maze without needing the server or Unity installed.

1. Move with the arrow keys or WASD, or use the on-screen directional controls.
2. Find the hidden snowflake checkpoint.
3. Choose **Send symbol to Unity**. Demo mode records the action locally in the web app.
4. Choose **Demo: simulate Unity activation** to test the exit reveal.
5. Reach the flag to record the Paraglider activation request.

## Integration boundary

The current API contract in `../docs/api.md` is explicitly marked as a draft. The web app's demo helper simulates the Unity activation locally; it is not a live Unity event. Before setting `GameApi:DemoMode` to `false`, the team needs to agree on the actual server endpoints, request payloads, event names, and WebSocket/SignalR transport. The server folder in the supplied repository is a placeholder, so end-to-end Unity/server communication cannot be verified from this repository yet.

The Unity project files were not modified as part of this web update.
