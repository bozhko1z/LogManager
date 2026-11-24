using LogManager.Models;
using LogManager.Services;

namespace LogManager.Managers;

// Singleton Pattern: one shared manager instance in this application.
public sealed class LogManager
{
    private static readonly Lazy<LogManager> _instance =
        new(() => new LogManager());

    private readonly object _writeLock = new();
    private readonly LogFileService _fileService;

    private LogManager() => _fileService = new LogFileService();

    public static LogManager Instance => _instance.Value;
    public string FilePath => _fileService.FilePath;

    public void WriteLog(LogEntry entry)
    {
        lock (_writeLock)
        {
            // FileShare.None additionally prevents simultaneous writers from other processes.
            _fileService.Append(entry);
        }
    }

    public List<LogEntry> ReadLogs()
    {
        lock (_writeLock)
        {
            return _fileService.ReadAll()
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
    }

    public List<LogEntry> ReadLogsFromFile(string filePath)
    {
        lock (_writeLock)
        {
            return new LogFileService().ReadFromFile(filePath);
        }
    }
}
