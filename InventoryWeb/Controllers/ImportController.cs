using InventoryLibrary.Services.data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        
        private readonly ImportService _importService;

        public ImportController(ImportService importService)
        {
            _importService = importService;
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

        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            var fileBytes = _importService.GenerateExampleTemplate();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "InventoryTemplate.xlsx");
        }
    }
}
