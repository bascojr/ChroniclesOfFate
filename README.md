# Chronicles of Fate

A text-based Fantasy RPG game inspired by Uma Musume's career mode. Embark on a 10-year adventure, train your character, make decisions, battle enemies, and discover unique stories through equipped Storybooks.

## 🎮 Game Features

### Core Mechanics
- **10-Year Journey**: 120 turns (1 month per turn) to build your character
- **5 Character Classes**: Warrior, Mage, Rogue, Cleric, Ranger
- **6 Core Stats**: Strength, Agility, Intelligence, Endurance, Charisma, Luck
- **Energy System**: Manage your energy to perform actions each turn

### Actions
- **Train**: Choose from 6 training scenarios to boost specific stats
- **Rest**: Recover energy and health
- **Explore**: Find gold, trigger events, gain random stat boosts
- **Battle**: Fight enemies in auto-simulated combat
- **Study**: Focus on Intelligence and experience gains

### Storybook System
- Equip up to **5 Storybooks** at a time
- Each book provides **stat bonuses**
- Books inject **unique random events** into your gameplay
- Unlock rare books through achievements

### Random Events & Decision Trees
- Events trigger based on your actions and equipped Storybooks
- Multiple choice responses with **stat checks**
- Success/failure outcomes based on your stats
- Chain events can trigger follow-up scenarios

### Auto-Battle System
- Stat-based combat calculations
- Class-specific combat bonuses
- Critical hits based on Luck
- Earn experience, gold, and reputation from victories

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: Blazor WebAssembly
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens

## 📦 Project Structure

```
ChroniclesOfFate/
├── src/
│   ├── ChroniclesOfFate.Core/          # Domain entities, DTOs, interfaces
│   ├── ChroniclesOfFate.Infrastructure/ # EF Core, repositories, services
│   ├── ChroniclesOfFate.API/           # REST API controllers
│   └── ChroniclesOfFate.Blazor/        # Blazor WASM frontend
└── tests/
    ├── ChroniclesOfFate.Core.Tests/
    └── ChroniclesOfFate.API.Tests/
```

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ChroniclesOfFate
   ```

2. **Update connection string** in `src/ChroniclesOfFate.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ChroniclesOfFate;Trusted_Connection=True;"
     }
   }
   ```

3. **Run database migrations**
   ```bash
   cd src/ChroniclesOfFate.API
   dotnet ef database update
   ```

4. **Start the API**
   ```bash
   cd src/ChroniclesOfFate.API
   dotnet run
   ```
   API will start at `https://localhost:7000`

5. **Start the Blazor frontend** (in a new terminal)
   ```bash
   cd src/ChroniclesOfFate.Blazor
   dotnet run
   ```
   Frontend will start at `https://localhost:7001`

6. **Open your browser** and navigate to `https://localhost:7001`

## 🎯 API Endpoints

### Authentication
- `POST /api/auth/register` - Create new account
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Refresh token

### Game
- `GET /api/game/sessions` - List saved games
- `POST /api/game/sessions` - Create new game
- `GET /api/game/sessions/{id}/state` - Get current game state
- `POST /api/game/sessions/{id}/turn` - Process turn action
- `GET /api/game/sessions/{id}/training` - Get available training
- `GET /api/game/sessions/{id}/enemies` - Get available enemies
- `POST /api/game/sessions/{sessionId}/events/{eventId}/choice/{choiceId}` - Process event choice

### Storybooks
- `GET /api/game/storybooks` - List all storybooks
- `POST /api/game/sessions/{id}/storybooks/equip` - Equip storybook
- `POST /api/game/sessions/{id}/storybooks/loadout` - Set full loadout

## 🎲 Game Balance

### Class Starting Stats
| Class   | STR | AGI | INT | END | CHA | LCK |
|---------|-----|-----|-----|-----|-----|-----|
| Warrior | 20  | 12  | 8   | 18  | 10  | 12  |
| Mage    | 8   | 12  | 22  | 10  | 14  | 14  |
| Rogue   | 12  | 22  | 14  | 10  | 12  | 18  |
| Cleric  | 10  | 10  | 18  | 16  | 16  | 10  |
| Ranger  | 14  | 20  | 12  | 14  | 10  | 14  |

### Energy Costs
- Training: 15-30 energy
- Explore: 15 energy
- Battle: 30 energy
- Study: 20 energy
- Rest: Recovers 30+ energy

### Seasonal Bonuses
- **Spring**: Endurance training +20%, Exploration +10%
- **Summer**: Endurance training +20%, Battle XP +15%
- **Autumn**: Arcane Studies +25%, Event chance +10%
- **Winter**: Rest recovery +20%, Study +15%

## 📝 Future Enhancements

- [ ] Leaderboards
- [ ] More storybooks and events
- [ ] Multiplayer guild system
- [ ] Achievement system
- [ ] Mobile-responsive design improvements
- [ ] Sound effects and music
- [ ] Save file export/import

## 📄 License

MIT License - feel free to use this project for learning or as a base for your own game!

---

Built with ❤️ using ASP.NET Core and Blazor
