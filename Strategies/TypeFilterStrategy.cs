using LogManager.Models;

namespace LogManager.Strategies;

public sealed class TypeFilterStrategy(LogType? type) : ILogFilterStrategy
{
    public IEnumerable<LogEntry> Filter(IEnumerable<LogEntry> logs) =>
        type is null ? logs : logs.Where(x => x.Type == type.Value);
}
