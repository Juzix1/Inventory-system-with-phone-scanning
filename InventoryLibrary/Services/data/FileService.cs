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



namespace InventoryLibrary.Services.data;

public class FileService : IFileService
{
    private readonly MyDbContext _context;

    public FileService(MyDbContext context, ISettingsService settings)
    {
        _context = context;
    }



   
    public async Task<Result> ImportInventoryItemsAsync(Stream excelStream)
    {
        var result = new Result();
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
        foreach(var item in items)
        {
            index ++;
            WriteRows(worksheet, item, index);
            
        }
        worksheet.Cells.AutoFitColumns();

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

        // Nagłówki
        

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public class Result
    {
        public int SuccessCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsSuccess => Errors.Count == 0;
    }

    private void WriteRows(ExcelWorksheet worksheet, InventoryItem item, int column)
    {

        worksheet.Cells[column,1].Value = $"{item.ItemType?.TypeName??"General"}";
        worksheet.Cells[column,2].Value = $"{item.itemName ?? ""}";
        worksheet.Cells[column,3].Value = $"{item.ItemTypeId.ToString() ?? ""}";
        worksheet.Cells[column,4].Value = $"{item.ItemCondition?.ConditionName ?? ""}";
        worksheet.Cells[column,5].Value = $"{item.itemWeight.ToString()??""}";
        worksheet.Cells[column,6].Value = $"{item.itemPrice.ToString()?? "0"}";
        worksheet.Cells[column,7].Value = $"{item.addedDate.ToString()?? ""}";
        worksheet.Cells[column,8].Value = $"{item.warrantyEnd.ToString()??""}";
        worksheet.Cells[column,9].Value = $"{item.lastInventoryDate.ToString()??""}";
        worksheet.Cells[column,15].Value = $"{item.personInCharge?.Email?? ""}";
        worksheet.Cells[column,16].Value = $"{item.Location?.RoomName?? ""}";
        worksheet.Cells[column,17].Value = $"{item.itemDescription?? ""}";
        switch (item)
        {
            case AGD agd:
                //AGD
                worksheet.Cells[column,10].Value = $"{agd.ModelName}";
                worksheet.Cells[column,11].Value = $"{agd.CPU}";
                worksheet.Cells[column,12].Value = $"{agd.RAM}";
                worksheet.Cells[column,13].Value = $"{agd.Storage}";
                worksheet.Cells[column,14].Value = $"{agd.Graphics}";
                break;
            case Furniture furniture:
                //Meble
                worksheet.Cells[column,10].Value = $"{furniture.FurnitureType}";

                break;
        }
        
    }

}
