using System;
using InventoryLibrary.Model.Logs;
using InventoryLibrary.Services.Interfaces;

namespace InventoryLibrary.Services.Logs;

public class LogReaderService: ILogReaderService
{
private readonly string _logPath = "logs";
    private readonly string _logFileName = "myapp";

    public async Task<List<LogEntry>> GetLogsAsync(int count = 100)
    {
        var logs = new List<LogEntry>();
        var logFiles = Directory.GetFiles(_logPath, "*.txt")
            .OrderByDescending(f => f)
            .Take(3);

        foreach (var file in logFiles)
        {
            if (File.Exists(file))
            {
                var lines = await File.ReadAllLinesAsync(file);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        logs.Add(ParseLogLine(line));
                    }
                }
            }
        }

        return logs.OrderByDescending(l => l.Timestamp).Take(count).ToList();
    }

    public async Task<List<LogEntry>> GetLogsByDateAsync(DateTime date)
    {
        var logs = new List<LogEntry>();
        var logFile = Path.Combine(_logPath, $"{_logFileName}-{date:dd-MM-yyyy}.txt");

        if (File.Exists(logFile))
        {
            var lines = await File.ReadAllLinesAsync(logFile);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    logs.Add(ParseLogLine(line));
                }
            }
        }

        return logs.OrderByDescending(l => l.Timestamp).ToList();
    }

    private LogEntry ParseLogLine(string line)
    {
        try
        {
            var timestampEnd = line.IndexOf('[');
            var timestamp = line.Substring(0, timestampEnd).Trim();
            
            var levelStart = line.IndexOf('[') + 1;
            var levelEnd = line.IndexOf(']');
            var level = line.Substring(levelStart, levelEnd - levelStart).Trim();
            
            var sourceStart = line.IndexOf('[', levelEnd) + 1;
            var sourceEnd = line.IndexOf(']', sourceStart);
            var source = sourceStart > 0 && sourceEnd > sourceStart 
                ? line.Substring(sourceStart, sourceEnd - sourceStart).Trim() 
                : "Unknown";
            
            var message = line.Substring(sourceEnd + 1).Trim();

            return new LogEntry
            {
                Timestamp = DateTime.Parse(timestamp),
                Level = level,
                Source = source,
                Message = message
            };
        }
        catch
        {
            return new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = "SYSTEM_INFO",
                Source = "system",
                Message = line
            };
        }
    }
}
