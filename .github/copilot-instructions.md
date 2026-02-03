## Purpose
Provide concise, repo-specific guidance for AI coding agents working on this ASP.NET Core + Blazor project.

## Big-picture architecture
- Backend: `src/ChroniclesOfFate.API` — ASP.NET Core 8 Web API exposing game endpoints (Auth, Admin, Game).
- Domain: `src/ChroniclesOfFate.Core` — domain entities, DTOs, enums, and services (game rules, combat, events).
- Persistence: `src/ChroniclesOfFate.Infrastructure` — EF Core `ApplicationDbContext`, repositories, UnitOfWork, and migrations.
- Frontend: `src/ChroniclesOfFate.Blazor` — Blazor WebAssembly UI calling the API via `Services/AdminApiService.cs` and other services.

Focus: changes to game rules usually happen in `Core/Services` (not in controllers). Controllers mostly map requests to Core services.

## Key files & entry points (examples)
- Game loop & action processing: `src/ChroniclesOfFate.Core/Services/TurnService.cs` and `BattleService.cs`.
- Event selection: `src/ChroniclesOfFate.Core/Services/RandomEventService.cs`.
- DB context & seeding: `src/ChroniclesOfFate.Infrastructure/Data/ApplicationDbContext.cs` and `Migrations/`.
- Auth: `src/ChroniclesOfFate.API/Controllers/AuthController.cs` (JWT flows).
- Frontend game UI: `src/ChroniclesOfFate.Blazor/Pages/Game.razor` and client services in `src/ChroniclesOfFate.Blazor/Services/`.

## Developer workflows
- Build all projects:
  - `dotnet build` at repo root.
- Run API locally:
  - `cd src/ChroniclesOfFate.API` then `dotnet run` (default port used by launchSettings; repo uses port 7000 in docs).
- Run Blazor frontend locally:
  - `cd src/ChroniclesOfFate.Blazor` then `dotnet run` (frontend docs reference port 7001).
- Run tests:
  - `dotnet test tests/ChroniclesOfFate.Core.Tests`.
- EF Migrations / DB:
  - Migrations live under `src/ChroniclesOfFate.Infrastructure/Migrations` — use standard `dotnet ef` commands in the Infrastructure project if you need new migrations.

Note: After backend code changes, restart the API process to pick up changes.

## Project-specific patterns and conventions
- Core is authoritative: business rules live in `Core/Services`. Prefer changing `TurnService.cs` / `BattleService.cs` over controller logic for gameplay changes.
- DTOs in `src/ChroniclesOfFate.Core/DTOs` are used across API and Blazor; keep DTO shape stable when evolving endpoints.
- Repositories + UnitOfWork pattern in `Infrastructure/` — use existing repositories instead of calling `ApplicationDbContext` directly from higher layers.
- Active skills are tracked per-battle using a `HashSet<int> usedActiveSkills` (see `BattleService.cs`): active skills only proc once per battle.
- Combat timing uses an attack-interval formula: `interval = max(500, 5000 / (1 + agility / 100))`. Implement changes to timing in `BattleService.cs` and `TurnService.cs`.
- Events use a weighted selection pattern (build weighted list, then roll once). See `RandomEventService.cs` for implementation details.

## Integration points & external dependencies
- SQL Server via EF Core — check `src/ChroniclesOfFate.Infrastructure` for connection strings and seeding.
- JWT bearer auth — handled in API startup and `AuthController.cs`.
- Blazor frontend calls the API via typed services under `Blazor/Services` — adjust service methods when changing API DTOs.

## When editing code — practical tips
- Small gameplay tweak: modify `Core/Services/TurnService.cs` or `BattleService.cs`, run unit tests, then run the API and play via `Blazor` UI to observe behavior.
- Adding a DB field: add property to entity in `Core/Entities`, update EF mapping and migrations in `Infrastructure`, update DTOs and API endpoints.
- Logging: game and combat logs are appended to domain entities (e.g., `BattleLog.cs`, `MessageLogEntry.cs`); prefer appending structured messages for UI parsing.

## Quick search hints
- Find game rules: search `Core/Services` for `Turn`, `Battle`, `RandomEvent`.
- Find DTOs: `Core/DTOs/GameDtos.cs`.
- Find controllers: `src/ChroniclesOfFate.API/Controllers/`.

## Safety & tests
- There are unit tests under `tests/ChroniclesOfFate.Core.Tests` — run them after any Core changes. Focus tests on deterministic logic (interval calculation, event weights, skill procs).

---
If any section is unclear or you want more examples (e.g., where to change skill proc messages or how to add a migration), tell me which area to expand. 