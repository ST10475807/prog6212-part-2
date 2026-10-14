# RaceDay API — VS Code startup guide

## Start the API

1. Install .NET 8 SDK and SQL Server LocalDB (or update the connection string for your SQL Server instance).
2. Open the `RaceDay-Part2-API` folder in VS Code — the folder should contain `RaceDay.sln`.
3. Select **Terminal → New Terminal**. If that command is hidden, use **Ctrl+Shift+`**.
4. Run `dotnet --version`. If Windows says `dotnet` is not recognized, install the .NET 8 SDK and restart VS Code.
5. Run the commands below from the folder containing `RaceDay.sln`:

```powershell
dotnet restore .\RaceDay.sln
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef migrations add InitialRaceDay --project .\RaceDay.Api\RaceDay.Api.csproj --startup-project .\RaceDay.Api\RaceDay.Api.csproj
dotnet ef database update --project .\RaceDay.Api\RaceDay.Api.csproj --startup-project .\RaceDay.Api\RaceDay.Api.csproj
dotnet run --project .\RaceDay.Api\RaceDay.Api.csproj
```

If `dotnet-ef` is already installed, run `dotnet tool update --global dotnet-ef --version 8.*` instead of the install command. If the migration already exists, skip the `migrations add` command and run `database update` only.

6. Keep the terminal open while the API is running. Open **http://localhost:5080/swagger** in Chrome. To stop the API, focus the terminal and press **Ctrl+C**.

## Common problems

- **`Couldn't find a project to run`**: run the command with `--project .\RaceDay.Api\RaceDay.Api.csproj` from the solution folder.
- **Port already in use**: change the port in `RaceDay.Api/Properties/launchSettings.json` and use the new port in the browser.
- **SQL Server connection error**: make sure LocalDB/SQL Server is installed and running, then check `RaceDayConnection` in `appsettings.json`.
- **Swagger not opening**: confirm the terminal still says `Now listening on: http://localhost:5080`; use the exact URL shown by the terminal. Do not close the terminal while testing.
- **Migration already exists**: do not add `InitialRaceDay` a second time; run `dotnet ef database update`.
