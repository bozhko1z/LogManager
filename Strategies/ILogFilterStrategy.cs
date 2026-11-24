using LogManager.Models;

namespace LogManager.Strategies;

// Strategy Pattern: interchangeable filtering algorithms.
public interface ILogFilterStrategy
{
    IEnumerable<LogEntry> Filter(IEnumerable<LogEntry> logs);
}
