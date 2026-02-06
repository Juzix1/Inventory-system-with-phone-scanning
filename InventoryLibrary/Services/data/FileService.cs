using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using InventoryWeb.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;
using Microsoft.Extensions.Logging;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.Location;



namespace InventoryLibrary.Services.data;

public class FileService : IFileService
{
    private readonly MyDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly IInventoryLogger<FileService> _logger;

    public FileService(MyDbContext context, IInventoryService inventoryService, IInventoryLogger<FileService> logger)
    {
        _context = context;
        _inventoryService = inventoryService;
        _logger = logger;
    }




    public async Task<Result> ImportInventoryItemsAsync(Stream excelStream)
    {
        var result = new Result();
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        _logger.LogInfo("Starting import of inventory items from Excel");
        try
        {
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet.Dimension == null)
            {
                result.Errors.Add("Worksheet is empty");
                _logger.LogError("Import failed: Worksheet is empty");
                return result;
            }
            var rowCount = worksheet.Dimension.Rows;
            var itemTypes = await _context.ItemTypes
                .ToDictionaryAsync(t => t.TypeName.ToLower(), t => t);
            var itemConditions = await _context.itemConditions
                .ToDictionaryAsync(c => c.ConditionName.ToLower(), c => c);
            var accounts = await _context.Accounts
                .ToDictionaryAsync(a => a.Email.ToLower(), a => a);
            var rooms = await _context.Rooms
                .Include(r => r.Department)
                .ToDictionaryAsync(r => r.RoomName.ToLower(), r => r);

            var itemsToAdd = new List<InventoryItem>();
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var itemTypeName = GetCellValue(worksheet, row, 1);
                    if (string.IsNullOrWhiteSpace(itemTypeName))
                        continue;
                    if (!itemTypes.TryGetValue(itemTypeName.ToLower(), out var itemType))
                    {
                        result.Errors.Add($"Row {row}: Unknown ItemType '{itemTypeName}'");
                        continue;
                    }
                    InventoryItem item = itemType.TypeName.ToLower() switch
                    {
                        "computer" or "electronics" or "agd" => CreateAGD(worksheet, row),
                        "furniture" => CreateFurniture(worksheet, row),
                        _ => CreateInventoryItem(worksheet, row)
                    };

                    var mappingResult = MapCommonFields(
                        item, worksheet, row,
                        itemTypes, itemConditions, accounts, rooms);

                    if (!mappingResult.IsSuccess)
                    {
                        result.Errors.Add($"Row {row}: {string.Join(", ", mappingResult.Errors)}");
                        continue;
                    }

                    await _inventoryService.CreateItemAsync(item);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {row}: {ex.Message}");
                    _logger.LogError($"Row {row}: Error importing item",ex);
                }
            }

            if (itemsToAdd.Any())
            {
                await _context.InventoryItems.AddRangeAsync(itemsToAdd);
                await _context.SaveChangesAsync();
                result.SuccessCount = itemsToAdd.Count;
                _logger.LogInfo($"Imported {itemsToAdd.Count} items successfully.");
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Critical Error: {ex.Message}");
            _logger.LogError("Critical error during import of inventory items",ex);
        }

        return result;
    }


    private InventoryItem CreateInventoryItem(ExcelWorksheet worksheet, int row)
    {
        return new InventoryItem();
    }

    private AGD CreateAGD(ExcelWorksheet worksheet, int row)
    {
        return new AGD
        {
            ModelName = GetCellValue(worksheet, row, 10),
            CPU = GetCellValue(worksheet, row, 11),
            RAM = GetCellValue(worksheet, row, 12),
            Storage = GetCellValue(worksheet, row, 13),
            Graphics = GetCellValue(worksheet, row, 14)
        };
    }

    private Furniture CreateFurniture(ExcelWorksheet worksheet, int row)
    {
        return new Furniture
        {
            FurnitureType = GetCellValue(worksheet, row, 10)
        };
    }

    private Result MapCommonFields(
    InventoryItem item,
    ExcelWorksheet worksheet,
    int row,
    Dictionary<string, ItemType> itemTypes,
    Dictionary<string, ItemCondition> itemConditions,
    Dictionary<string, Account> accounts,
    Dictionary<string, Room> rooms)
    {
        var result = new Result();

        try
        {
            // ItemType
            var itemTypeName = GetCellValue(worksheet, row, 1);
            if (!string.IsNullOrWhiteSpace(itemTypeName) &&
                itemTypes.TryGetValue(itemTypeName.ToLower(), out var itemType))
            {
                item.ItemType = itemType;
                item.ItemTypeId = itemType.Id;
            }
            else
            {
                result.Errors.Add($"Invalid ItemType: {itemTypeName}");
                return result;
            }

            // ItemName
            item.itemName = GetCellValue(worksheet, row, 2);
            if (string.IsNullOrWhiteSpace(item.itemName))
            {
                result.Errors.Add("ItemName is required");
            }

            // Condition
            var conditionName = GetCellValue(worksheet, row, 4);
            if (!string.IsNullOrWhiteSpace(conditionName) &&
                itemConditions.TryGetValue(conditionName.ToLower(), out var condition))
            {
                item.ItemCondition = condition;
            }

            // Weight
            if (double.TryParse(GetCellValue(worksheet, row, 5), out var weight))
            {
                item.itemWeight = weight;
            }

            // Price
            if (double.TryParse(GetCellValue(worksheet, row, 6), out var price))
            {
                item.itemPrice = price;
            }

            // AddedDate
            if (DateTime.TryParse(GetCellValue(worksheet, row, 7), out var addedDate))
            {
                item.addedDate = addedDate;
            }
            else
            {
                item.addedDate = DateTime.Now;
            }

            // WarrantyEnd
            if (DateTime.TryParse(GetCellValue(worksheet, row, 8), out var warrantyEnd))
            {
                item.warrantyEnd = warrantyEnd;
            }

            // LastInventoryDate
            if (DateTime.TryParse(GetCellValue(worksheet, row, 9), out var lastInventory))
            {
                item.lastInventoryDate = lastInventory;
            }

            // PersonEmail
            var personEmail = GetCellValue(worksheet, row, 15);
            if (!string.IsNullOrWhiteSpace(personEmail) &&
                accounts.TryGetValue(personEmail.ToLower(), out var account))
            {
                item.personInCharge = account;
            }

            // RoomName
            var roomName = GetCellValue(worksheet, row, 16);
            if (!string.IsNullOrWhiteSpace(roomName) &&
                rooms.TryGetValue(roomName.ToLower(), out var room))
            {
                item.Location = room;
            }

            // Description
            item.itemDescription = GetCellValue(worksheet, row, 17);

            result.IsSuccess = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Mapping error: {ex.Message}");
            _logger.LogError($"Row {row}: Error mapping common fields",ex);
        }

        return result;
    }

    private string GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        return worksheet.Cells[row, col].Value?.ToString()?.Trim() ?? string.Empty;
    }

    public async Task<byte[]> ExportInventoryItemsAsync(List<InventoryItem> items)
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Inventory Status");

        var headers = new[]
        {
            "ItemType", "ItemName","ItemTypeId", "Condition",
            "Weight", "Price", "AddedDate", "WarrantyEnd", "LastInventoryDate",
            "ModelName/FurnitureType", "CPU", "RAM", "Storage", "Graphics",
            "PersonEmail", "RoomName", "Description"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        var index = 1;
        foreach (var item in items)
        {
            index++;
            WriteRows(worksheet, item, index);

        }
        worksheet.Cells.AutoFitColumns();
        _logger.LogInfo($"Exported {items.Count} items to Excel.");
        return await package.GetAsByteArrayAsync();

    }
    public byte[] GenerateExampleTemplate()
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;


        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Inventory Template");

        var headers = new[]
{
            "ItemType", "ItemName","ItemTypeId", "Condition",
            "Weight", "Price", "AddedDate", "WarrantyEnd", "LastInventoryDate",
            "ModelName/FurnitureType", "CPU", "RAM", "Storage", "Graphics",
            "PersonEmail", "RoomName", "Description"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        var items = new List<InventoryItem>();
        var generalType = new ItemType { Id = 1, TypeName = "NoType" };
        var furnitureType = new ItemType { Id = 3, TypeName = "Furniture" };
        var electronicsType = new ItemType { Id = 2, TypeName = "AGD" };

        var goodCondition = new ItemCondition { Id = 1, ConditionName = "Good" };
        var excellentCondition = new ItemCondition { Id = 2, ConditionName = "New" };
        var fairCondition = new ItemCondition { Id = 3, ConditionName = "Lost" };

        var person1 = new Account { Email = "john.doe@company.com" };
        var person2 = new Account { Email = "jane.smith@company.com" };

        var room1 = new Department { DepartmentName = "Wydział Informatyki", Rooms = new List<Room> { new Room { RoomName = "Dziekanat" } } };
        var room2 = new Department { DepartmentName = "Wydział Robotyki", Rooms = new List<Room> { new Room { RoomName = "Sala 112" } } };
        var room3 = new Department { DepartmentName = "Wydział Prawa", Rooms = new List<Room> { new Room { RoomName = "Aula15" } } };

        items.Add(new AGD
        {
            ItemType = electronicsType,
            itemName = "Dell Latitude 5520",
            ItemTypeId = 1,
            ItemCondition = excellentCondition,
            itemWeight = 1.8,
            itemPrice = 3500.00,
            addedDate = DateTime.Now.AddMonths(-6),
            warrantyEnd = DateTime.Now.AddYears(2),
            lastInventoryDate = DateTime.Now.AddDays(-156),
            ModelName = "Latitude 5520",
            CPU = "Intel Core i7-1185G7",
            RAM = "16GB DDR4",
            Storage = "512GB NVMe SSD",
            Graphics = "Intel Iris Xe",
            itemDescription = "High-performance business laptop for development work"
        });

        items.Add(new AGD
        {
            ItemType = electronicsType,
            itemName = "HP EliteDesk 800 G6",
            ItemTypeId = 1,
            ItemCondition = goodCondition,
            itemWeight = 5.2,
            itemPrice = 4200.00,
            addedDate = DateTime.Now.AddMonths(-12),
            warrantyEnd = DateTime.Now.AddYears(1),
            lastInventoryDate = DateTime.Now.AddDays(-45),
            ModelName = "EliteDesk 800 G6",
            CPU = "Intel Core i9-10900",
            RAM = "32GB DDR4",
            Storage = "1TB NVMe SSD",
            Graphics = "NVIDIA RTX 3060",
            itemDescription = "Powerful workstation for graphics and video editing"
        });

        int row = 2;
        foreach (var item in items)
        {
            WriteRows(worksheet, item, row);
            row++;
        }


        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public class Result
    {
        public int SuccessCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsSuccess { get; set; }
    }

    private void WriteRows(ExcelWorksheet worksheet, InventoryItem item, int column)
    {

        worksheet.Cells[column, 1].Value = $"{item.ItemType?.TypeName ?? "General"}";
        worksheet.Cells[column, 2].Value = $"{item.itemName ?? ""}";
        worksheet.Cells[column, 3].Value = $"{item.ItemTypeId.ToString() ?? ""}";
        worksheet.Cells[column, 4].Value = $"{item.ItemCondition?.ConditionName ?? ""}";
        worksheet.Cells[column, 5].Value = $"{item.itemWeight.ToString() ?? ""}";
        worksheet.Cells[column, 6].Value = $"{item.itemPrice.ToString() ?? "0"}";
        worksheet.Cells[column, 7].Value = $"{item.addedDate.ToString() ?? ""}";
        worksheet.Cells[column, 8].Value = $"{item.warrantyEnd.ToString() ?? ""}";
        worksheet.Cells[column, 9].Value = $"{item.lastInventoryDate.ToString() ?? ""}";
        worksheet.Cells[column, 15].Value = $"{item.personInCharge?.Email ?? ""}";
        worksheet.Cells[column, 16].Value = $"{item.Location?.RoomName ?? ""}";
        worksheet.Cells[column, 17].Value = $"{item.itemDescription ?? ""}";
        switch (item)
        {
            case AGD agd:
                worksheet.Cells[column, 10].Value = $"{agd.ModelName}";
                worksheet.Cells[column, 11].Value = $"{agd.CPU}";
                worksheet.Cells[column, 12].Value = $"{agd.RAM}";
                worksheet.Cells[column, 13].Value = $"{agd.Storage}";
                worksheet.Cells[column, 14].Value = $"{agd.Graphics}";
                break;
            case Furniture furniture:
                worksheet.Cells[column, 10].Value = $"{furniture.FurnitureType}";

                break;
        }

    }

}
