using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.Location;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IConditionService _conditionService;
        private readonly ISettingsService _settingsService;

        public InventoryController(MyDbContext context, IInventoryService inventoryService, IConditionService conditionService, ISettingsService settingsService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _conditionService = conditionService;
            _settingsService = settingsService;
        }

        [HttpGet("{id}/barcode")]
        public async Task<ActionResult> getBarcodeImage(int id)
        {

            var item = await _context.InventoryItems.FindAsync(id);

            if (item == null)
            {
                return NotFound("Item not found.");
            }
            var barcodeHandler = new BarcodeGenerator();
            // Ensure we have a barcode value to encode. If missing, generate one and persist it.

            var outputPath = Path.Combine("Images", $"{item.Id}.jpg");
            var barcodeText = barcodeHandler.GenerateBarcodeNumber(item.Id, item.ItemTypeId);
            var companyName = await _settingsService.GetSettingValue<string>("CompanyName") ?? string.Empty;
            barcodeHandler.GenerateBarcode(barcodeText, companyName, outputPath);

            if (System.IO.File.Exists(outputPath))
            {
                var imageBytes = System.IO.File.ReadAllBytes(outputPath);
                barcodeHandler.deleteBarcodeImage();
                return File(imageBytes, "image/jpeg");
            }
            else
            {
                return NotFound("Barcode image not found.");
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAllInventoryItems()
        {
            var items = await _inventoryService.GetAllItemsAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryItem>> GetInventoryItemById(int id)
        {
            try
    {
        var item = await _context.InventoryItems
            .AsNoTracking()
            .Include(i => i.Location)
            .Include(i => i.ItemType)
            .Include(i => i.ItemCondition)
            .Include(i => i.personInCharge)
            .FirstOrDefaultAsync(i => i.Id == id);
        
        if (item == null)
        {
            return NotFound(new { message = "Item not found" });
        }
        
        var dto = new InventoryDTO(
            id: item.Id,
            itemName: item.itemName,
            itemDescription: item.itemDescription,
            itemType: item.ItemType?.TypeName,
            ItemConditionId: item.ItemConditionId,
            itemWeight: item.itemWeight,
            itemPrice: item.itemPrice,
            addedDate: item.addedDate,
            warrantyEnd: item.warrantyEnd,
            lastInventoryDate: item.lastInventoryDate,
            personInChargeId: item.PersonInChargeId,
            room: item.Location?.RoomName,
            stocktakeId: item.StocktakeId
        );
        
        return Ok(dto);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Internal server error" });
    }
        }

    }
}
