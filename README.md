# Log Manager (C# / .NET 8)

Windows Forms desktop application for centralized log management.

## Requirements
- Windows
- .NET 8 SDK
- Visual Studio 2022 (or `dotnet` CLI)

## Run
1. Open `LogManager.csproj` in Visual Studio.
2. Restore packages (there are no external NuGet dependencies).
3. Run with F5.

Or from this folder:
```bash
dotnet run
```

## Features
- INFO, WARNING, ERROR, DEBUG and SUCCESS log types.
- Add, persist, read, filter by type and search by message.
- Centralized JSON Lines file in `%LOCALAPPDATA%/LogManager/Logs/application.jsonl`.
- In-process synchronization with `lock` and exclusive file opening with `FileShare.None`.

## Design patterns
- **Singleton**: `Managers/LogManager.cs` exposes one instance through `LogManager.Instance`.
- **Factory**: `Factories/LogFactory.cs` creates validated `LogEntry` objects.
- **Strategy**: `Strategies/ILogFilterStrategy.cs` defines interchangeable filtering algorithms; type and search filters implement it.

## Notes on concurrency
`lock` serializes calls made through this application's singleton instance. `FileShare.None` prevents another process from opening the log file while an append operation is active. If multiple independent application processes write at exactly the same time, the second process can receive an `IOException`; production systems could add retry/backoff.

## Storage format
Each line in `application.jsonl` is a separate JSON-serialized `LogEntry`, which allows the program to read records incrementally.
