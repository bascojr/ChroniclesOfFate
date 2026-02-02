using ChroniclesOfFate.Core.Interfaces;

namespace ChroniclesOfFate.Core.Services;

/// <summary>
/// Thread-safe random number generation service
/// Extracted for testability - can be mocked in unit tests
/// </summary>
public class RandomService : IRandomService
{
    private static readonly ThreadLocal<Random> _random = new(() => 
        new Random(Interlocked.Increment(ref _seed)));
    
    private static int _seed = Environment.TickCount;

    public int Next(int maxValue)
    {
        return _random.Value!.Next(maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
        return _random.Value!.Next(minValue, maxValue);
    }

    public double NextDouble()
    {
        return _random.Value!.NextDouble();
    }

    public bool RollChance(double probability)
    {
        return NextDouble() < probability;
    }

    public int RollDice(int sides, int count = 1)
    {
        int total = 0;
        for (int i = 0; i < count; i++)
        {
            total += Next(1, sides + 1);
        }
        return total;
    }
}
