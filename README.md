# RaceDay System — Part 2: ASP.NET Core REST API

This project continues the Part 1 RaceDay plan in `st10475807 poe part1.pdf`. It uses the same core tables and endpoint names: `Users`, `Events`, `Categories`, `EventCategories`, `Enrolments`, and `Results`.

## Technology
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core Code First + SQL Server
- Password hashing with ASP.NET Core `PasswordHasher<TUser>`
- JWT bearer authentication with role claims
- Swagger/OpenAPI
- xUnit tests
- GitHub Actions build/test workflow

## Roles
- **Organizer**: manages only events they created, assigns categories, views enrolments for their events, and records/updates results.
- **Participant**: views events/categories, enrols in categories, views and cancels their own enrolments, and views their own results.
- Public endpoints: event and category browsing, registration and login.
- Protected endpoints require a bearer token. Use the token returned by login in Swagger's **Authorize** dialog.

## Prerequisites
Install Visual Studio 2022 with the ASP.NET and web development workload, .NET 8 SDK, and SQL Server / LocalDB. For another SQL Server instance, update `ConnectionStrings:RaceDayConnection` in `RaceDay.Api/appsettings.json`.

## Run locally (VS Code or Visual Studio)

1. Install the **.NET 8 SDK** and SQL Server LocalDB (or SQL Server Express). In VS Code, open the folder that contains `RaceDay.sln`.
2. Open a terminal at that folder and check the SDK:
   ```powershell
   dotnet --version
   ```
3. Restore packages:
   ```powershell
   dotnet restore .\RaceDay.sln
   ```
4. Install the Entity Framework command-line tool once, if it is not already installed:
   ```powershell
   dotnet tool install --global dotnet-ef --version 8.*
   ```
   If it is already installed, update it with `dotnet tool update --global dotnet-ef --version 8.*`.
5. Create the database schema and apply it (the connection string creates a separate `RaceDayPart2DB` database to avoid conflicts with a Part 1 database):
   ```powershell
   dotnet ef migrations add InitialRaceDay --project .\RaceDay.Api\RaceDay.Api.csproj --startup-project .\RaceDay.Api\RaceDay.Api.csproj
   dotnet ef database update --project .\RaceDay.Api\RaceDay.Api.csproj --startup-project .\RaceDay.Api\RaceDay.Api.csproj
   ```
6. Start the API:
   ```powershell
   dotnet run --project .\RaceDay.Api\RaceDay.Api.csproj
   ```
   The included launch profile sets Development mode and uses port **5080**. Open **http://localhost:5080/swagger**. The root URL redirects to Swagger. If port 5080 is already in use, change `applicationUrl` in `RaceDay.Api/Properties/launchSettings.json` to another free port and use that same port in the browser.
7. In Swagger, register an Organizer and a Participant using `POST /api/auth/register`, then log in using `POST /api/auth/login`. Copy the `accessToken`, click **Authorize**, and paste the token. Use a Participant token for enrolment/profile actions and an Organizer token for event/category/result management.

### Database and configuration notes

- The default connection uses Windows SQL Server LocalDB. If LocalDB is not installed, edit `ConnectionStrings:RaceDayConnection` in `RaceDay.Api/appsettings.json` to point to an installed SQL Server instance.
- The JWT key in `appsettings.json` is for local coursework development only. Replace it with a private random secret for your own machine and never commit production secrets.
- If you change the EF models after creating the initial migration, add another migration and run `dotnet ef database update` again.
- This project enables Swagger for straightforward local coursework testing. Do not expose this development configuration directly to the public internet.

## Main endpoints
| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register Organizer or Participant |
| POST | `/api/auth/login` | Public | Login and return JWT |
| GET/PUT | `/api/users/me` | Authenticated | View/update own profile |
| GET | `/api/events` | Public | List events; optional `status`, `eventType`, `fromDate`, and `toDate` query filters |
| GET | `/api/events/{id}` | Public | Event details and categories |
| POST | `/api/events` | Organizer | Create event |
| PUT/DELETE | `/api/events/{id}` | Organizer (owner) | Update/delete own event |
| GET/POST/PUT/DELETE | `/api/categories` and `/api/categories/{id}` | Public read; Organizer write | Manage categories |
| GET | `/api/events/{eventId}/categories` | Public | Categories assigned to an event |
| POST/DELETE | `/api/events/{eventId}/categories/{categoryId}` | Organizer (owner) | Assign/remove category |
| POST | `/api/events/{eventId}/categories/{categoryId}/enrolments` | Participant | Enrol in event category |
| GET/DELETE | `/api/enrolments/me`, `/api/enrolments/{id}` | Participant | Own enrolments / cancellation |
| GET | `/api/events/{eventId}/enrolments` | Organizer (owner) | View event enrolments |
| GET | `/api/results/me` | Participant | Own results |
| GET/POST/PUT | `/api/enrolments/{enrolmentId}/result` | Own event Organizer or own Participant read; Organizer write | View/capture/update result |

## Example request bodies

See [`docs/API_EXAMPLES.md`](docs/API_EXAMPLES.md) for ready-to-use Swagger JSON examples for registration, login, events, categories, enrolments, and results.

## Unit tests
Run:
```bash
dotnet test RaceDay.sln
```
The tests currently verify password hashing/verification and accepted registration roles, with basic model checks. Extend them with integration tests for every controller and authorization scenario before final submission.

## GitHub Actions
The workflow at `.github/workflows/dotnet.yml` restores, builds, and tests the solution on push and pull request. After pushing the project to GitHub:
1. Open **Actions** in the repository.
2. Open the latest workflow run and confirm it succeeds.
3. Capture a screenshot showing the green successful run and add it here under `docs/screenshots/ci-success.png`.
4. Replace this instruction with the actual screenshot. Do not claim a green build until GitHub has run it successfully.

## Submission checklist
- [ ] Push at least 20 meaningful commits to your own repository.
- [ ] Confirm endpoint names and schema against the lecturer-approved Part 1 specification.
- [ ] Run migrations and test on your SQL Server.
- [ ] Add screenshots of Swagger endpoints, tests, and successful GitHub Actions run.
- [ ] Record an unlisted YouTube demonstration covering code structure, endpoints, authentication/roles, and tests; add the real URL here.
- [ ] Submit your GitHub repository URL on ARC.

## Important alignment notes
The Part 2 model adds `EventType` (Run/Walk/Cycle) and includes event time, venue, location, and capacity fields. It uses a separate `RaceDayPart2DB` database so its EF migration does not collide with a database created directly from the Part 1 SQL script. The Part 1 endpoint table says login returns a token, so this implementation uses JWT bearer authentication rather than server-side ASP.NET session cookies. This is appropriate for a REST API and matches the Part 1 contract, but confirm the session wording with your lecturer.
