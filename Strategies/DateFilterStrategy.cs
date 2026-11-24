using LogManager.Models;

namespace LogManager.Strategies;

public sealed class DateFilterStrategy(DateTime date) : ILogFilterStrategy
{
    public IEnumerable<LogEntry> Filter(IEnumerable<LogEntry> logs) =>
        logs.Where(x => x.Timestamp.Date == date.Date);
}
