using System;

namespace InventoryLibrary.Model.Data;

public class ImageUploadResult
{
    public bool Success { get; set; }
    public string ImageUrl { get; set; }
    public string ImagePath { get; set; }
    public string ImageId { get; set; }
    public string Message { get; set; }
}