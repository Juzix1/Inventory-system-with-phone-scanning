using System;
using System.ComponentModel;
using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace InventoryLibrary.Services.data;

public class ImportService
{
    private readonly MyDbContext _context;

    public ImportService(MyDbContext context)
    {
        _context = context;
        
    }

    public async Task<ImportResult> ImportInventoryItemsAsync(Stream excelStream)
    {
        var result = new ImportResult();
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;


        try
        {
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];

            if(worksheet.Dimension == null)
            {
                result.Errors.Add("worksheet is empty");
                return result;
            }

            var rowCount = worksheet.Dimension.Rows;

            var itemTypes = await _context.ItemTypes.ToDictionaryAsync(t => t.TypeName, t=> t.Id);
            var itemConditions = await _context.itemConditions.ToDictionaryAsync(c => c.ConditionName, c => c.Id);
            var accounts = await _context.Accounts.ToDictionaryAsync(a => a.Email, a=>a.Id);
            var rooms = await _context.Rooms.Include(r => r.Department).ToDictionaryAsync(r => r.RoomName, r=> r.Id);

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var itemTypeName = GetCellValue(worksheet, row, 1);

                    if(string.IsNullOrWhiteSpace(itemTypeName))
                        continue;

                    InventoryItem item = itemTypeName.ToLower() switch
                    {
                        "agd" => CreateAGD(worksheet, row),
                        "furniture" => CreateFurniture(worksheet, row),
                        _ => CreateInventoryItem(worksheet, row)
                    };

                    //wspólne pola
                    MapCommonFields(item, worksheet, row, itemTypes,itemConditions, accounts, rooms);

                    _context.InventoryItems.Add(item);
                    result.SuccessCount++;

                }catch(Exception ex)
                {
                    result.Errors.Add($"Row {row}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
        }catch(Exception ex)
        {
            result.Errors.Add($"Critical Error: {ex.Message}");
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
            ModelName = GetCellValue(worksheet, row, 11) ?? "",
                CPU = GetCellValue(worksheet, row, 12) ?? "",
                RAM = GetCellValue(worksheet, row, 13) ?? "",
                Storage = GetCellValue(worksheet, row, 14) ?? "",
                Graphics = GetCellValue(worksheet, row, 15) ?? ""
        };
    }

    private Furniture CreateFurniture(ExcelWorksheet worksheet, int row)
    {
        return new Furniture
        {
            FurnitureType = GetCellValue(worksheet, row, 11) ?? ""
        };
    }

    private void MapCommonFields(
        InventoryItem item,
        ExcelWorksheet worksheet,
        int row,
        Dictionary<string, int> itemTypes,
        Dictionary<string, int> itemConditions,
        Dictionary<string, int> accounts,
        Dictionary<string, int> rooms)
        {
            item.itemName = GetCellValue(worksheet, row, 2) ?? throw new Exception("Brak nazwy przedmiotu");
            item.Barcode = GetCellValue(worksheet, row, 3) ?? "";
            item.itemDescription = GetCellValue(worksheet, row, 4);
            //type
            var itemTypeName = GetCellValue(worksheet, row,1);
            if(!string.IsNullOrEmpty(itemTypeName) && itemTypes.ContainsKey(itemTypeName))
        {
            item.ItemTypeId = itemTypes[itemTypeName];
        }
        else
        {
            item.ItemTypeId = 1;
        }
        //Condition
        var conditionName = GetCellValue(worksheet, row, 5);
        if(!string.IsNullOrEmpty(conditionName) && itemConditions.ContainsKey(conditionName))
        {
            item.ItemConditionId = itemConditions[conditionName];
        }
        else
        {
            item.ItemConditionId = 1;
        }

        item.itemWeight = GetDoubleValue(worksheet, row, 6);
        item.itemPrice = GetDoubleValue(worksheet, row, 7);
        item.addedDate = GetDateValue(worksheet, row, 8) ?? DateTime.Now;
        item.warrantyEnd = GetDateValue(worksheet, row, 9) ?? DateTime.Now.AddYears(1);
        item.lastInventoryDate = GetDateValue(worksheet, row, 10) ?? DateTime.Now;

        var personEmail = GetCellValue(worksheet, row, 16);
        if (!string.IsNullOrEmpty(personEmail) && accounts.ContainsKey(personEmail))
        {
            item.PersonInChargeId = accounts[personEmail];
        }

        var roomName = GetCellValue(worksheet, row, 17);
        if(!string.IsNullOrEmpty(roomName) && rooms.ContainsKey(roomName))
        {
            item.RoomId = rooms[roomName];
        }
    }

    private string? GetCellValue(ExcelWorksheet worksheet, int row, int col)
    {
        return worksheet.Cells[row, col].Value?.ToString()?.Trim();
    }

    private double GetDoubleValue(ExcelWorksheet worksheet, int row, int col)
    {
        var value = worksheet.Cells[row, col].Value;
        if (value == null) return 0;
        
        if (double.TryParse(value.ToString(), out double result))
            return result;
        
        return 0;
    }

    private DateTime? GetDateValue(ExcelWorksheet worksheet, int row, int col)
    {
        var value = worksheet.Cells[row, col].Value;
        if (value == null) return null;

        if (value is DateTime dt)
            return dt;

        if (DateTime.TryParse(value.ToString(), out DateTime result))
            return result;

        return null;
    }

    public byte[] GenerateExampleTemplate()
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;


        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Inventory Template");

        // Nagłówki
        var headers = new[]
        {
            "ItemType", "ItemName", "Barcode", "Description", "Condition",
            "Weight", "Price", "AddedDate", "WarrantyEnd", "LastInventoryDate",
            "ModelName/FurnitureType", "CPU", "RAM", "Storage", "Graphics",
            "PersonEmail", "RoomName"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Przykładowe dane
        worksheet.Cells[2, 1].Value = "AGD";
        worksheet.Cells[2, 2].Value = "Laptop Dell";
        worksheet.Cells[2, 3].Value = "123456789";
        worksheet.Cells[2, 4].Value = "Laptop służbowy";
        worksheet.Cells[2, 5].Value = "Good";
        worksheet.Cells[2, 6].Value = 2.5;
        worksheet.Cells[2, 7].Value = 3500.00;
        worksheet.Cells[2, 8].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[2, 9].Value = DateTime.Now.AddYears(2).ToString("yyyy-MM-dd");
        worksheet.Cells[2, 10].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[2, 11].Value = "Dell Latitude 5520";
        worksheet.Cells[2, 12].Value = "Intel i5-1135G7";
        worksheet.Cells[2, 13].Value = "16GB";
        worksheet.Cells[2, 14].Value = "512GB SSD";
        worksheet.Cells[2, 15].Value = "Intel Iris Xe";
        worksheet.Cells[2, 16].Value = "admin@example.com";
        worksheet.Cells[2, 17].Value = "Office 101";

        worksheet.Cells[3, 1].Value = "Furniture";
        worksheet.Cells[3, 2].Value = "Biurko Ikea";
        worksheet.Cells[3, 3].Value = "987654321";
        worksheet.Cells[3, 4].Value = "Biurko z regulacją wysokości";
        worksheet.Cells[3, 5].Value = "New";
        worksheet.Cells[3, 6].Value = 25.0;
        worksheet.Cells[3, 7].Value = 1200.00;
        worksheet.Cells[3, 8].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[3, 9].Value = DateTime.Now.AddYears(5).ToString("yyyy-MM-dd");
        worksheet.Cells[3, 10].Value = DateTime.Now.ToString("yyyy-MM-dd");
        worksheet.Cells[3, 11].Value = "Desk";

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsSuccess => Errors.Count == 0;
    }

}
