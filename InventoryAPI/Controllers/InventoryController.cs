using InventoryAPI.Data;
using InventoryAPI.Model;
using InventoryAPI.Model.Inventory;
using InventoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IInventoryService _inventoryService;

        public InventoryController(MyDbContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
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
            var outputPath = Path.Combine("Images", $"{item.Barcode}.jpg");
            barcodeHandler.GenerateBarcode(item.Barcode, outputPath);

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

        [HttpPost("add")]
        public async Task<IActionResult> AddInventoryItem()
        {
            var barcodeGen = new BarcodeGenerator();
            var barcodeNumber = await barcodeGen.GenerateBarcodeNumber(_context);

            var computer = new Computer
            {
                itemName = "Laptop MSI f123",
                Barcode = barcodeNumber,
                itemCategory = "Elektronika",
                itemCondition = "Nowy",
                quantity = 1,
                itemWeight = 2.5,
                itemPrice = 4500,
                addedDate = DateTime.Now,
                warrantyEnd = DateTime.Now.AddYears(2),
                lastInventoryDate = DateTime.Now,
                itemLocation = "Pokój 103",
                ModelName = "MSI AERO 2",
                CPU = "AMD Ryzen 5 5600g",
                RAM = "8GB",
                Storage = "216GB SSD",
                Graphics = "AMD Vega 9"
            };

            var furniture = new Furniture
            {
                FurnitureType = "Biurko",
                itemName = "Biurko IKEA",
                Barcode = barcodeNumber,
                itemCategory = "Meble",
                itemCondition = "Nowy",
                quantity = 1,
                itemWeight = 20.0,
                itemPrice = 300,
                addedDate = DateTime.Now,
                warrantyEnd = DateTime.Now.AddYears(1),
                lastInventoryDate = DateTime.Now,
                itemLocation = "Pokój 103",
            };

            _context.InventoryItems.Add(furniture);
            await _context.SaveChangesAsync(); // Use async SaveChanges
            return Ok();
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
            var item  = await _inventoryService.GetItemByIdAsync(id);
            

            return Ok(item);
        }
    }
}
