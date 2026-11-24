using LogManager.Models;

namespace LogManager.Strategies;

public sealed class SearchFilterStrategy(string term) : ILogFilterStrategy
{
    public IEnumerable<LogEntry> Filter(IEnumerable<LogEntry> logs) =>
        string.IsNullOrWhiteSpace(term)
            ? logs
            : logs.Where(x => x.Message.Contains(term, StringComparison.OrdinalIgnoreCase));
}
