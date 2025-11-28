using System;
using System.Text.Json;
using InventoryLibrary.Data;
using InventoryLibrary.Services.Interfaces;
using InventoryWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace InventoryWeb.Services;

public class SettingsService : ISettingsService
{
    private readonly MyDbContext _context;
    public SettingsService(MyDbContext context)
    {
        _context = context;
    }
    public async Task<List<Setting>> GetSettings()
    {
        return await _context.Settings.ToListAsync();
    }

    public async Task UpdateSetting(SettingsModel model)
    {
        var settings = await _context.Settings.ToListAsync();

        var fileStoragePathSetting = settings.Find(s => s.Key == "FileStoragePath");
        if (fileStoragePathSetting != null)
        {
            fileStoragePathSetting.Value = model.FileStoragePath;
        }
        else
        {
            _context.Settings.Add(new Setting { Key = "FileStoragePath", Value = model.FileStoragePath });
        }

        var maxFileSizeSetting = settings.Find(s => s.Key == "MaxFileSize");
        if (maxFileSizeSetting != null)
        {
            maxFileSizeSetting.Value = (model.MaxFileSizeMB * 1024 * 1024).ToString();
        }
        else
        {
            _context.Settings.Add(new Setting { Key = "MaxFileSize", Value = (model.MaxFileSizeMB * 1024 * 1024).ToString() });
        }

        var enableNotificationsSetting = settings.Find(s => s.Key == "EnableNotifications");
        if (enableNotificationsSetting != null)
        {
            enableNotificationsSetting.Value = model.EnableNotifications.ToString();
        }
        else
        {
            _context.Settings.Add(new Setting { Key = "EnableNotifications", Value = model.EnableNotifications.ToString() });
        }

        var companyNameSetting = settings.Find(s => s.Key == "CompanyName");
        if (companyNameSetting != null)
        {
            companyNameSetting.Value = model.CompanyName;
        }
        else
        {
            _context.Settings.Add(new Setting { Key = "CompanyName", Value = model.CompanyName });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string> GetImageStoragePath()
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == "FileStoragePath");
        return setting != null ? setting.Value : string.Empty; 
    }
}
