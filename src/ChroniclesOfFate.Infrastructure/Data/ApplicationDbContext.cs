using ChroniclesOfFate.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChroniclesOfFate.Infrastructure.Data;

/// <summary>
/// Entity Framework Core DbContext for Chronicles of Fate
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Storybook> Storybooks => Set<Storybook>();
    public DbSet<CharacterStorybook> CharacterStorybooks => Set<CharacterStorybook>();
    public DbSet<RandomEvent> RandomEvents => Set<RandomEvent>();
    public DbSet<EventChoice> EventChoices => Set<EventChoice>();
    public DbSet<GameEvent> GameEvents => Set<GameEvent>();
    public DbSet<Enemy> Enemies => Set<Enemy>();
    public DbSet<BattleLog> BattleLogs => Set<BattleLog>();
    public DbSet<TrainingScenario> TrainingScenarios => Set<TrainingScenario>();
    public DbSet<MessageLogEntry> MessageLogEntries => Set<MessageLogEntry>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<CharacterSkill> CharacterSkills => Set<CharacterSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("Player");
        });

        // GameSession configuration
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SessionName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EndingType).HasMaxLength(100);
            
            entity.HasOne(e => e.Character)
                .WithOne(c => c.GameSession)
                .HasForeignKey<Character>(c => c.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Character configuration
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            
            entity.Property(e => e.Strength).HasDefaultValue(10);
            entity.Property(e => e.Agility).HasDefaultValue(10);
            entity.Property(e => e.Intelligence).HasDefaultValue(10);
            entity.Property(e => e.Endurance).HasDefaultValue(10);
            entity.Property(e => e.Charisma).HasDefaultValue(10);
            entity.Property(e => e.Luck).HasDefaultValue(10);
            
            entity.Property(e => e.MaxEnergy).HasDefaultValue(100);
            entity.Property(e => e.MaxHealth).HasDefaultValue(100);
            entity.Property(e => e.CurrentEnergy).HasDefaultValue(100);
            entity.Property(e => e.CurrentHealth).HasDefaultValue(100);
            entity.Property(e => e.Level).HasDefaultValue(1);
            entity.Property(e => e.CurrentYear).HasDefaultValue(1);
            entity.Property(e => e.CurrentMonth).HasDefaultValue(1);
        });

        // Storybook configuration
        modelBuilder.Entity<Storybook>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Theme).HasMaxLength(50);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.UnlockCondition).HasMaxLength(500);
        });

        // CharacterStorybook configuration
        modelBuilder.Entity<CharacterStorybook>(entity =>
        {
            entity.HasKey(e => new { e.CharacterId, e.StorybookId });
            
            entity.HasOne(e => e.Character)
                .WithMany(c => c.EquippedStorybooks)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Storybook)
                .WithMany(s => s.CharacterStorybooks)
                .HasForeignKey(e => e.StorybookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.SlotPosition).IsRequired();
        });

        // RandomEvent configuration
        modelBuilder.Entity<RandomEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.TriggerActions).HasMaxLength(100);
            entity.Property(e => e.TriggerSeasons).HasMaxLength(100);
            entity.Property(e => e.StatRequirements).HasMaxLength(500);
            
            entity.HasOne(e => e.Storybook)
                .WithMany(s => s.Events)
                .HasForeignKey(e => e.StorybookId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // EventChoice configuration
        modelBuilder.Entity<EventChoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ResultDescription).HasMaxLength(1000);
            entity.Property(e => e.FailureDescription).HasMaxLength(1000);
            entity.Property(e => e.StatRequirements).HasMaxLength(500);

            entity.HasOne(e => e.RandomEvent)
                .WithMany(r => r.Choices)
                .HasForeignKey(e => e.RandomEventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.GrantSkill)
                .WithMany()
                .HasForeignKey(e => e.GrantSkillId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.FailureGrantSkill)
                .WithMany()
                .HasForeignKey(e => e.FailureGrantSkillId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // GameEvent configuration
        modelBuilder.Entity<GameEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ResultSummary).HasMaxLength(500);
            
            entity.HasOne(e => e.Character)
                .WithMany(c => c.EventHistory)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.RandomEvent)
                .WithMany()
                .HasForeignKey(e => e.RandomEventId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ChosenOption)
                .WithMany()
                .HasForeignKey(e => e.ChosenOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Enemy configuration
        modelBuilder.Entity<Enemy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.EnemyType).HasMaxLength(50);
            entity.Property(e => e.LootTable).HasMaxLength(2000);
            entity.Property(e => e.ActiveSeasons).HasMaxLength(100);
            entity.Property(e => e.Abilities).HasMaxLength(1000);
        });

        // BattleLog configuration
        modelBuilder.Entity<BattleLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BattleNarrative).HasMaxLength(5000);
            
            entity.HasOne(e => e.Character)
                .WithMany(c => c.BattleHistory)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Enemy)
                .WithMany()
                .HasForeignKey(e => e.EnemyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TrainingScenario configuration
        modelBuilder.Entity<TrainingScenario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.TrainingNarrative).HasMaxLength(1000);
            entity.Property(e => e.BonusSeasons).HasMaxLength(100);
        });

        // MessageLogEntry configuration
        modelBuilder.Entity<MessageLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.StatChangesJson).HasMaxLength(1000);

            entity.HasOne(e => e.GameSession)
                .WithMany(g => g.MessageLog)
                .HasForeignKey(e => e.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.GameSessionId, e.CreatedAt });
        });

        // Skill configuration
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.ActiveNarrative).HasMaxLength(500);
        });

        // CharacterSkill configuration
        modelBuilder.Entity<CharacterSkill>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Character)
                .WithMany(c => c.Skills)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Skill)
                .WithMany(s => s.CharacterSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.Property(e => e.AcquisitionSource).HasMaxLength(200);

            // Ensure character can't have duplicate skills
            entity.HasIndex(e => new { e.CharacterId, e.SkillId }).IsUnique();
        });
    }
}
