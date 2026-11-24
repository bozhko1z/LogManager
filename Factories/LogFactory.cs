using LogManager.Models;

namespace LogManager.Factories;

// Factory Pattern: centralizes creation of log entries.
public static class LogFactory
{
    public static LogEntry Create(LogType type, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Log message cannot be empty.", nameof(message));

        return new LogEntry
        {
            Timestamp = DateTime.Now,
            Type = type,
            Message = message.Trim()
        };
    }
}
