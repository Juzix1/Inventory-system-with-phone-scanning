using System;
using System.Drawing;
using System.Net.Http.Json;
using InventoryLibrary.Data;
using InventoryLibrary.Model.Data;
using InventoryLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace InventoryLibrary.Services.data;

public class ImageService : IImageService
{
    private readonly MyDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;


    public ImageService(HttpClient httpClient, ISettingsService settingsService, MyDbContext context)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
        _context = context;
    }

    public async Task<ImageUploadResult> SendImageAsync(Stream imageStream, string fileName, int? inventoryItemId = null)
    {
        try
        {
            var storagePath = await _settingsService.GetImageStoragePath();

            if (string.IsNullOrEmpty(storagePath))
            {
                throw new InvalidOperationException("Image storage path is not configured in settings");
            }

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var fullPath = Path.Combine(storagePath, uniqueFileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Do NOT update DB here — caller will set Model.imagePath and persist on submit.

            return new ImageUploadResult
            {
                Success = true,
                ImageUrl = uniqueFileName,
                ImagePath = fullPath,
                ImageId = uniqueFileName,
                Message = "Image uploaded successfully"
            };
        }
        catch (Exception ex)
        {
            return new ImageUploadResult
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}"
            };
        }
    }
    
    public async Task<bool> DeleteImageAsync(string imagePath)
    {
        try
        {
            var storagePath = await _settingsService.GetImageStoragePath();
            var path = Path.Combine(storagePath, imagePath);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}


