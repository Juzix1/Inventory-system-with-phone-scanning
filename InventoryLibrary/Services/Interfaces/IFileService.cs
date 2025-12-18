using System;
using InventoryLibrary.Model.Inventory;
using System.IO;
using static InventoryLibrary.Services.data.FileService;

namespace InventoryLibrary.Services.Interfaces;

public interface IFileService
{

    Task<Result> ImportInventoryItemsAsync(Stream excelStream);
    Task<byte[]> ExportInventoryItemsAsync(List<InventoryItem> items);
    byte[] GenerateExampleTemplate();
}
