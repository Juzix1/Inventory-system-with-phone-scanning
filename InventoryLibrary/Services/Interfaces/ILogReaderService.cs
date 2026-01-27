using System;
using InventoryLibrary.Model.Logs;

namespace InventoryLibrary.Services.Interfaces;

public interface ILogReaderService
{
    Task<List<LogEntry>> GetLogsAsync(int count = 100);
    Task<List<LogEntry>> GetLogsByDateAsync(DateTime date);
}
