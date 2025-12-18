using System;
using InventoryLibrary.Model.Data;

namespace InventoryLibrary.Services.Interfaces;

public interface IImageService
{
    Task<ImageUploadResult> SendImageAsync(Stream imageStream, string fileName, int inventoryItemId);
    Task<bool> DeleteImageAsync(string imagePath);
}
