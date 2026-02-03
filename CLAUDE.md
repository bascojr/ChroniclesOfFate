# Chronicles of Fate - Project Context

## Overview
A text-based Fantasy RPG inspired by Uma Musume's career mode. 10-year journey (120 turns), train characters, make decisions in events, battle enemies, and equip Storybooks for unique events.

## Tech Stack
- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: Blazor WebAssembly
- **Database**: SQL Server with Entity Framework Core
- **Auth**: JWT Bearer tokens

## Project Structure
```
src/
├── ChroniclesOfFate.Core/           # Domain entities, DTOs, interfaces, services
├── ChroniclesOfFate.Infrastructure/ # EF Core, repositories, UnitOfWork
├── ChroniclesOfFate.API/            # REST API controllers
└── ChroniclesOfFate.Blazor/         # Blazor WASM frontend
```

## Key Files

### Core Services
| File | Purpose |
|------|---------|
| `Core/Services/TurnService.cs` | Main game loop, processes all turn actions (Train, Rest, Explore, Battle) |
| `Core/Services/BattleService.cs` | Time-based combat simulation |
| `Core/Services/RandomEventService.cs` | Event triggering and choice processing |
| `Core/Services/TrainingService.cs` | Training scenario execution |

### DTOs
| File | Purpose |
|------|---------|
| `Core/DTOs/GameDtos.cs` | All game data transfer objects |

### Frontend
| File | Purpose |
|------|---------|
| `Blazor/Pages/Game.razor` | Main game UI, action buttons, event display, combat log |
| `Blazor/wwwroot/css/app.css` | All game styling |

### Data
| File | Purpose |
|------|---------|
| `Infrastructure/Repositories/Repositories.cs` | All repository implementations |
| `Infrastructure/Data/ApplicationDbContext.cs` | EF Core context and seeding |

## Combat System (Time-Based)

### Attack Interval Formula
```
interval = max(500, 5000 / (1 + agility / 100))
```
- Base interval: 5000ms
- Minimum interval: 500ms (hard cap)
- Maximum battle duration: 60 seconds

### Example Intervals
| Agility | Attack Interval |
|---------|-----------------|
| 0       | 5.00 seconds    |
| 100     | 2.50 seconds    |
| 200     | 1.67 seconds    |
| 500     | 0.83 seconds    |

### Combat Flow
1. Calculate attack intervals for both combatants
2. First attacks happen after first interval (not at time 0)
3. Track `playerNextAttack` and `enemyNextAttack` timers
4. Whoever's timer is lower attacks first
5. Continue until one dies or 60s max duration

## Skill System

### Skill Types
- **Passive**: Always active during combat (Evasion, CriticalChance, DamageReduction, LifeSteal, CounterAttack, Thorns)
- **Active**: Triggered during combat, proc once per battle (Power Strike, etc.)
- **Bonus**: Adventure-wide passive bonuses (GoldGain, ExperienceGain, HealthRegen, etc.)

### Active Skill Behavior
- Each active skill can only proc ONCE per battle
- Tracked via `HashSet<int> usedActiveSkills` in combat loop
- Skill procs are logged to adventure log with catchy messages

## Event System

### Weighted Random Selection
Events use weighted random selection (not individual rolls):
1. Build weighted list of all eligible events
2. Apply rarity boosts and modifiers
3. Roll once and select one event based on cumulative weights

### Event Triggering
- **Explore**: 100% chance, prefers higher rarity, doubles non-common rates
- **Train**: 25% chance
- **Rest**: 15% chance

### Follow-up Events
Event choices can have `FollowUpEventId` which triggers chain events after a choice.

## Explore Action

### Rewards
- **Experience**: Base 50-500 (scales with level: `50 + (level-1) * 25`), up to 50% bonus roll
- **Stat Gains**: 0-3 to ALL stats randomly
- **Events**: 100% trigger chance with double rate for non-common events

### Energy Cost
- 50 energy per explore

## Mini Events
20% chance after any successful action (lines 403-542 in TurnService):
- 70% positive (stat gain, gold, energy, reputation, experience)
- 30% negative (stat loss, gold loss, energy drain, health loss)

## Running the Project

### API (Port 7000)
```bash
cd src/ChroniclesOfFate.API
dotnet run
```

### Frontend (Port 7001)
```bash
cd src/ChroniclesOfFate.Blazor
dotnet run
```

### Build
```bash
dotnet build
```

## Important Notes
- After code changes, **restart the API** for changes to take effect
- Event choices display rewards/penalties before selection
- Combat log shows timestamps, not round numbers
- Skills panel accessible via button in stats panel
