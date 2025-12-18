using System;
using InventoryWeb.Models;

namespace InventoryLibrary.Services.Interfaces;

public interface ISettingsService
{
    Task<List<Setting>> GetSettings();
    Task UpdateSetting(SettingsModel model);
    Task<string> GetImageStoragePath();
}