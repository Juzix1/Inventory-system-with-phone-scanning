using System;
using InventoryLibrary.Model.Logs;
using InventoryLibrary.Services.Interfaces;

namespace InventoryLibrary.Services.Logs;

public class InventoryLogger<T> : IInventoryLogger<T>
{
    private readonly string _logPath = "logs";
    private readonly string _logFileName = "InventoryLog";
    private readonly string _source;
    private static readonly object _lock = new object();

    public InventoryLogger()
    {
        Directory.CreateDirectory(_logPath);
        _source = typeof(T).Name;
    }

    private string GetLogFilePath()
    {
        return Path.Combine(_logPath, $"{_logFileName}-{DateTime.Now:yyyy-MM-dd}.txt");
    }

    public void LogInfo(string message)
    {
        WriteLog("INFO", message);
    }

    public void LogWarning(string message)
    {
        WriteLog("WARNING", message);
    }

    public void LogError(string message, Exception ex = null)
    {
        var fullMessage = message;
        if (ex != null)
        {
            fullMessage += $"\n    Exception: {ex.Message}";
        }
        WriteLog("ERROR", fullMessage);
    }

    private void WriteLog(string level, string message)
    {
        var logEntry = $"{DateTime.Now:dd-MM-yyyy HH:mm} [{level}] [{_source}] {message}";
        
        lock (_lock)
        {
            File.AppendAllText(GetLogFilePath(), logEntry + Environment.NewLine);
        }
    }
}