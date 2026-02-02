using ChroniclesOfFate.Core.Interfaces;
using ChroniclesOfFate.Infrastructure.Data;
using ChroniclesOfFate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ChroniclesOfFate.Infrastructure;

/// <summary>
/// Unit of Work pattern implementation for transaction management
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IUserRepository? _users;
    private IGameSessionRepository? _gameSessions;
    private ICharacterRepository? _characters;
    private IStorybookRepository? _storybooks;
    private IRandomEventRepository? _randomEvents;
    private IEventChoiceRepository? _eventChoices;
    private ITrainingScenarioRepository? _trainingScenarios;
    private IEnemyRepository? _enemies;
    private IBattleLogRepository? _battleLogs;
    private IGameEventRepository? _gameEvents;
    private IMessageLogEntryRepository? _messageLogEntries;
    private ISkillRepository? _skills;
    private ICharacterSkillRepository? _characterSkills;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => 
        _users ??= new UserRepository(_context);
    
    public IGameSessionRepository GameSessions => 
        _gameSessions ??= new GameSessionRepository(_context);
    
    public ICharacterRepository Characters => 
        _characters ??= new CharacterRepository(_context);
    
    public IStorybookRepository Storybooks => 
        _storybooks ??= new StorybookRepository(_context);
    
    public IRandomEventRepository RandomEvents => 
        _randomEvents ??= new RandomEventRepository(_context);
    
    public IEventChoiceRepository EventChoices => 
        _eventChoices ??= new EventChoiceRepository(_context);
    
    public ITrainingScenarioRepository TrainingScenarios => 
        _trainingScenarios ??= new TrainingScenarioRepository(_context);
    
    public IEnemyRepository Enemies => 
        _enemies ??= new EnemyRepository(_context);
    
    public IBattleLogRepository BattleLogs => 
        _battleLogs ??= new BattleLogRepository(_context);
    
    public IGameEventRepository GameEvents =>
        _gameEvents ??= new GameEventRepository(_context);

    public IMessageLogEntryRepository MessageLogEntries =>
        _messageLogEntries ??= new MessageLogEntryRepository(_context);

    public ISkillRepository Skills =>
        _skills ??= new SkillRepository(_context);

    public ICharacterSkillRepository CharacterSkills =>
        _characterSkills ??= new CharacterSkillRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
