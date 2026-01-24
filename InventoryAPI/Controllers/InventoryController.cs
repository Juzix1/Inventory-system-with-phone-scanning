using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.Location;
using InventoryLibrary.Services.Interfaces;
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
        private readonly IConditionService _conditionService;

        public InventoryController(MyDbContext context, IInventoryService inventoryService, IConditionService conditionService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _conditionService = conditionService;
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
            barcodeHandler.GenerateBarcode(barcodeHandler.GenerateBarcodeNumber(item.Id,item.ItemTypeId), outputPath);

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

        // [HttpPost("add")]
        // public async Task<IActionResult> AddInventoryItem()
        // {
        //     var barcodeGen = new BarcodeGenerator();
        //     var barcodeNumber = await barcodeGen.GenerateBarcodeNumber(_context);

        //     // var agd = new AGD
        //     // {
        //     //     itemName = "Laptop MSI f123",
        //     //     Barcode = barcodeNumber,
        //     //     imagePath = "image1.png",
        //     //     itemCondition = "Nowy",
        //     //     ItemTypeId = 1,
        //     //     quantity = 1,
        //     //     itemWeight = 2.5,
        //     //     itemPrice = 4500,
        //     //     addedDate = DateTime.Now,
        //     //     warrantyEnd = DateTime.Now.AddYears(2),
        //     //     lastInventoryDate = DateTime.Now,
        //     //     itemLocation = "Pokój 103",
        //     //     ModelName = "MSI AERO 2",
        //     //     CPU = "AMD Ryzen 5 5600g",
        //     //     RAM = "8GB",
        //     //     Storage = "216GB SSD",
        //     //     Graphics = "AMD Vega 9"
        //     // };
        //     var type = await _context.ItemTypes.FindAsync(1);
        //     Department department = new Department
        //     {
        //         DepartmentName = "PANS",
        //         DepartmentLocation = "32sd-323s",
        //     };
        //     Room room = new Room
        //     {
        //         RoomName = "Magazyn",
        //         Department = department
        //     };
        //     _context.Departments.Add(department);
        //     _context.Rooms.Add(room);
        //     var item = new InventoryItem
        //     {
        //         itemName = "Laptop",
        //         Barcode = barcodeNumber,
        //         ItemType = type,
        //         addedDate = DateTime.Now,
        //         lastInventoryDate = DateTime.Now,
        //         Location = room
        //     };

        //     var furniture = new Furniture
        //     {
        //         FurnitureType = "Biurko",
        //         itemName = "Biurko IKEA",
        //         imagePath = "image2.png",
        //         Barcode = barcodeNumber,
        //         itemWeight = 20.0,
        //         itemPrice = 300,
        //         addedDate = DateTime.Now,
        //         warrantyEnd = DateTime.Now.AddYears(1),
        //         lastInventoryDate = DateTime.Now,
        //     };

        //     _context.InventoryItems.Add(item);
        //     await _context.SaveChangesAsync();
        //     return Ok();
        // }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAllInventoryItems()
        {
            var items = await _inventoryService.GetAllItemsAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryItem>> GetInventoryItemById(int id)
        {
            var item = await _inventoryService.GetItemByIdAsync(id);


            return Ok(item);
        }

    }
}
