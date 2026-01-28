using System.Security.Claims;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StocktakeController : ControllerBase
    {
        private readonly IStocktakeService _stocktakeService;
        private readonly IAccountsService _accountService;
        public StocktakeController(IStocktakeService stocktakeService, IAccountsService accountService)
        {
            _stocktakeService = stocktakeService;
            _accountService = accountService;
        }


        [HttpGet("my-items")]
        public async Task<ActionResult<IEnumerable<InventoryDTO>>> GetMyItems()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }


                var stocktakes = await _stocktakeService.GetStocktakesByAccount(userId);
                stocktakes = stocktakes.Where(i => i.Status == StockTakeStatus.InProgress);
                var allItems = stocktakes
                .SelectMany(stock => stock.ItemsToCheck)
                .DistinctBy(item => item.Id)
                .ToList();

                var items = allItems.Select(item => new InventoryDTO(
            id: item.Id,
            itemName: item.itemName,
            itemDescription: item.itemDescription,
            itemType: item.ItemType.TypeName,
            ItemConditionId: item.ItemConditionId,
            itemWeight: item.itemWeight,
            itemPrice: item.itemPrice,
            addedDate: item.addedDate,
            warrantyEnd: item.warrantyEnd,
            lastInventoryDate: item.lastInventoryDate,
            personInChargeId: item.PersonInChargeId,
            room: item.Location == null
                ? ""
                : $"{item?.Location?.RoomName} ({item?.Location?.Department.DepartmentName})",
            stocktakeId: item.StocktakeId,
            imagePath: item.imagePath
        ))
        .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error loading items", error = ex.Message });
            }
        }

        [HttpPost("{stocktakeId}/mark-item/{itemId}")]
        public async Task<ActionResult> MarkItemAsChecked(int stocktakeId, int itemId)
        {
            try
            {
                // Pobierz userId z tokenu
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }
                string checkedBy = userId.ToString();

                await _stocktakeService.MarkItemAsChecked(stocktakeId, itemId, checkedBy);
                var stocktake = await _stocktakeService.GetStocktakeById(stocktakeId);

                if (stocktake == null)
                {
                    return NotFound(new { message = $"Stocktake with ID {stocktakeId} not found" });
                }

                return Ok(new
                {
                    message = "Item marked as checked successfully",
                    stocktakeId = stocktakeId,
                    itemId = itemId,
                    checkedCount = stocktake.CheckedItemIdList.Count,
                    totalItems = stocktake.AllItems,
                    isCompleted = stocktake.Status == StockTakeStatus.Completed,
                    progress = $"{stocktake.CheckedItemIdList.Count}/{stocktake.AllItems}"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error marking item as checked", error = ex.Message });
            }
        }
        
        [HttpGet("{stocktakeId}/progress")]
        public async Task<ActionResult> GetStocktakeProgress(int stocktakeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }
                var stocktake = await _stocktakeService.GetStocktakeById(stocktakeId);

                if (stocktake == null)
                {
                    return NotFound(new { message = $"Stocktake with ID {stocktakeId} not found" });
                }

                return Ok(new
                {
                    stocktakeId = stocktake.Id,
                    status = stocktake.Status.ToString(),
                    checkedCount = stocktake.CheckedItemIdList.Count,
                    totalItems = stocktake.AllItems,
                    progress = stocktake.AllItems > 0
                        ? (double)stocktake.CheckedItemIdList.Count / stocktake.AllItems * 100
                        : 0,
                    isCompleted = stocktake.Status == StockTakeStatus.Completed,
                    checkedItemIds = stocktake.CheckedItemIdList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting stocktake progress", error = ex.Message });
            }
        }

        private InventoryDTO MapToDTO(InventoryItem item)
        {
            return new InventoryDTO
            (
                id: item.Id,
                itemName: item.itemName,
                itemDescription: item.itemDescription,
                itemType: item.ItemType.TypeName,
                itemPrice: item.itemPrice,
                itemWeight: item.itemWeight,
                ItemConditionId: item.ItemConditionId,
                room: item.Location?.RoomName,
                personInChargeId: item.PersonInChargeId,
                addedDate: item.addedDate,
                lastInventoryDate: item.lastInventoryDate,
                warrantyEnd: item.warrantyEnd,
                stocktakeId: item.StocktakeId,
                imagePath: item.imagePath
            );
        }
    }
}

