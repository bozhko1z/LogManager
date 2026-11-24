namespace LogManager.Models;

public sealed class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogType Type { get; set; }
    public string Message { get; set; } = "";

    public override string ToString() =>
        $"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Type.ToString().ToUpperInvariant()}] {Message}";
}
