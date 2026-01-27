using System;
using InventoryLibrary.Model.Logs;

namespace InventoryLibrary.Services.Interfaces;

public interface IInventoryLogger<T>
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception ex = null);
}
