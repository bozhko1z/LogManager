using System.Text.Json;
using LogManager.Models;

namespace LogManager.Services;

// Stores all logs centrally as JSON Lines (one JSON object per line).
public sealed class LogFileService
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public LogFileService(string? filePath = null)
    {
        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LogManager", "Logs");
        Directory.CreateDirectory(directory);
        _filePath = filePath ?? Path.Combine(directory, "application.jsonl");
    }

    public string FilePath => _filePath;

    public void Append(LogEntry entry)
    {
        string json = JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine;

        // FileShare.None prevents another process from opening the file while writing.
        // The in-process lock is provided by LogManager.
        using var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream);
        writer.Write(json);
        writer.Flush();
    }

    public List<LogEntry> ReadAll()
    {
        if (!File.Exists(_filePath)) return new List<LogEntry>();

        // Exclusive access also avoids reading a partially-written line.
        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);
        var result = new List<LogEntry>();

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var item = JsonSerializer.Deserialize<LogEntry>(line, JsonOptions);
                if (item is not null) result.Add(item);
            }
            catch (JsonException)
            {
                // Ignore malformed lines and continue reading the remaining log.
            }
        }
        return result;
    }

    public List<LogEntry> ReadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Log file not found.", filePath);

        var result = new List<LogEntry>();

        foreach (string line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var entry = JsonSerializer.Deserialize<LogEntry>(line);

                if (entry != null)
                    result.Add(entry);
            }
            catch (JsonException)
            {
                // Skip invalid lines
            }
        }

        return result.OrderByDescending(x => x.Timestamp).ToList();
    }
}
