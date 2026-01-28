using System;

namespace InventoryLibrary.Model.Logs;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string Source { get; set; }
    public string Message { get; set; }
}
