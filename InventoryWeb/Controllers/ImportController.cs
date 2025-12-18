using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.data;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        
        private readonly IFileService _importService;
        private readonly MyDbContext _context;

        public ImportController(IFileService importService, MyDbContext context)
        {
            _importService = importService;
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new {message = "File was not sent"});
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                return BadRequest(new {message = "Unsupported file format. Send Excel file"});
            }

            using var stream = file.OpenReadStream();
            var result = await _importService.ImportInventoryItemsAsync(stream);

            if (result.IsSuccess)
            {
                return Ok(new
                {
                    message = $"successfully Imported {result.SuccessCount} items",
                    successCount = result.SuccessCount
                });
            }
            else
            {
                return Ok(new
                {
                    message = $"Imported {result.SuccessCount} items with errors",
                    successCount = result.SuccessCount,
                    errors = result.Errors
                });
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadInventoryItems()
        {
            List<InventoryItem> items = await _context.InventoryItems
                .Include(i => i.ItemType)
                .Include(i => i.ItemCondition)
                .Include(i => i.Location)
                .ToListAsync();
            var fileBytes = await _importService.ExportInventoryItemsAsync(items);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InventoryStatus.xlsx");
        }

        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            var fileBytes = _importService.GenerateExampleTemplate();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InventoryTemplate.xlsx");
        }
    }
}
