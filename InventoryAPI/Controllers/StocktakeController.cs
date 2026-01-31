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
    [Authorize (Roles = "Admin, Moderator")]
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

                // Pobierz sprawdzone przedmioty dla wszystkich aktywnych inwentaryzacji
                var checkedItemIds = stocktakes
                    .SelectMany(s => s.CheckedItems)
                    .Select(ci => ci.InventoryItemId)
                    .ToHashSet();

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

                // Sprawdź czy użytkownik ma uprawnienia
                var isAuthorized = await _stocktakeService.IsUserAuthorized(stocktakeId, userId);
                if (!isAuthorized && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _stocktakeService.MarkItemAsChecked(stocktakeId, itemId, checkedBy);
                var stocktake = await _stocktakeService.GetStocktakeById(stocktakeId);

                if (stocktake == null)
                {
                    return NotFound(new { message = $"Stocktake with ID {stocktakeId} not found" });
                }

                // Policz sprawdzone przedmioty
                var checkedCount = stocktake.CheckedItems.Count;

                return Ok(new
                {
                    message = "Item marked as checked successfully",
                    stocktakeId = stocktakeId,
                    itemId = itemId,
                    checkedCount = checkedCount,
                    totalItems = stocktake.AllItems,
                    isCompleted = stocktake.Status == StockTakeStatus.Completed,
                    progress = $"{checkedCount}/{stocktake.AllItems}",
                    progressPercentage = stocktake.AllItems > 0 
                        ? Math.Round((double)checkedCount / stocktake.AllItems * 100, 2) 
                        : 0
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

        [HttpDelete("{stocktakeId}/unmark-item/{itemId}")]
        public async Task<ActionResult> UnmarkItemAsChecked(int stocktakeId, int itemId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Sprawdź uprawnienia
                var isAuthorized = await _stocktakeService.IsUserAuthorized(stocktakeId, userId);
                if (!isAuthorized && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _stocktakeService.UnmarkItemAsChecked(stocktakeId, itemId);
                var stocktake = await _stocktakeService.GetStocktakeById(stocktakeId);

                if (stocktake == null)
                {
                    return NotFound(new { message = $"Stocktake with ID {stocktakeId} not found" });
                }

                var checkedCount = stocktake.CheckedItems.Count;

                return Ok(new
                {
                    message = "Item unmarked successfully",
                    stocktakeId = stocktakeId,
                    itemId = itemId,
                    checkedCount = checkedCount,
                    totalItems = stocktake.AllItems,
                    progress = $"{checkedCount}/{stocktake.AllItems}",
                    progressPercentage = stocktake.AllItems > 0 
                        ? Math.Round((double)checkedCount / stocktake.AllItems * 100, 2) 
                        : 0
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error unmarking item", error = ex.Message });
            }
        }

        [HttpPost("{stocktakeId}/mark-multiple")]
        public async Task<ActionResult> MarkMultipleItems(int stocktakeId, [FromBody] List<int> itemIds)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var isAuthorized = await _stocktakeService.IsUserAuthorized(stocktakeId, userId);
                if (!isAuthorized && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                if (itemIds == null || !itemIds.Any())
                {
                    return BadRequest(new { message = "No items provided" });
                }

                await _stocktakeService.MarkMultipleItemsAsChecked(stocktakeId, itemIds, userId.ToString());
                var stocktake = await _stocktakeService.GetStocktakeById(stocktakeId);

                if (stocktake == null)
                {
                    return NotFound(new { message = $"Stocktake with ID {stocktakeId} not found" });
                }

                var checkedCount = stocktake.CheckedItems.Count;

                return Ok(new
                {
                    message = $"Successfully marked {itemIds.Count} items as checked",
                    stocktakeId = stocktakeId,
                    markedItems = itemIds.Count,
                    checkedCount = checkedCount,
                    totalItems = stocktake.AllItems,
                    isCompleted = stocktake.Status == StockTakeStatus.Completed,
                    progress = $"{checkedCount}/{stocktake.AllItems}",
                    progressPercentage = stocktake.AllItems > 0 
                        ? Math.Round((double)checkedCount / stocktake.AllItems * 100, 2) 
                        : 0
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
                return StatusCode(500, new { message = "Error marking multiple items", error = ex.Message });
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

                // Policz sprawdzone przedmioty
                var checkedCount = stocktake.CheckedItems.Count;
                var uncheckedItems = await _stocktakeService.GetUncheckedItems(stocktakeId);

                return Ok(new
                {
                    stocktakeId = stocktake.Id,
                    name = stocktake.Name,
                    description = stocktake.Description,
                    status = stocktake.Status.ToString(),
                    startDate = stocktake.StartDate,
                    endDate = stocktake.EndDate,
                    checkedCount = checkedCount,
                    uncheckedCount = uncheckedItems.Count,
                    totalItems = stocktake.AllItems,
                    progress = stocktake.AllItems > 0
                        ? Math.Round((double)checkedCount / stocktake.AllItems * 100, 2)
                        : 0,
                    isCompleted = stocktake.Status == StockTakeStatus.Completed,
                    checkedItemIds = stocktake.CheckedItems.Select(ci => ci.InventoryItemId).ToList(),
                    daysRemaining = (stocktake.EndDate - DateTime.Now).Days
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting stocktake progress", error = ex.Message });
            }
        }

        [HttpGet("{stocktakeId}/statistics")]
        public async Task<ActionResult> GetStocktakeStatistics(int stocktakeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var stats = await _stocktakeService.GetStocktakeStatistics(stocktakeId);

                return Ok(new
                {
                    stocktakeId = stocktakeId,
                    totalItems = stats.TotalItems,
                    checkedItems = stats.CheckedItems,
                    uncheckedItems = stats.UncheckedItems,
                    progressPercentage = Math.Round(stats.ProgressPercentage, 2),
                    daysRemaining = stats.DaysRemaining,
                    isOverdue = stats.IsOverdue,
                    averageItemsPerDay = Math.Round(stats.AverageItemsPerDay, 2)
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting statistics", error = ex.Message });
            }
        }

        [HttpGet("{stocktakeId}/unchecked-items")]
        public async Task<ActionResult> GetUncheckedItems(int stocktakeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var isAuthorized = await _stocktakeService.IsUserAuthorized(stocktakeId, userId);
                if (!isAuthorized && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var uncheckedItems = await _stocktakeService.GetUncheckedItems(stocktakeId);
                var items = uncheckedItems.Select(item => MapToDTO(item)).ToList();

                return Ok(new
                {
                    stocktakeId = stocktakeId,
                    uncheckedCount = items.Count,
                    items = items
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting unchecked items", error = ex.Message });
            }
        }

        [HttpGet("{stocktakeId}/checked-items")]
        public async Task<ActionResult> GetCheckedItemsDetails(int stocktakeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var isAuthorized = await _stocktakeService.IsUserAuthorized(stocktakeId, userId);
                if (!isAuthorized && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var checkedItems = await _stocktakeService.GetCheckedItemsDetails(stocktakeId);

                return Ok(new
                {
                    stocktakeId = stocktakeId,
                    checkedCount = checkedItems.Count,
                    items = checkedItems.Select(ci => new
                    {
                        inventoryItemId = ci.InventoryItemId,
                        checkedDate = ci.CheckedDate,
                        checkedByUserId = ci.CheckedByUserId
                    }).ToList()
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting checked items", error = ex.Message });
            }
        }

        private InventoryDTO MapToDTO(InventoryItem item)
        {
            return new InventoryDTO
            (
                id: item.Id,
                itemName: item.itemName,
                itemDescription: item.itemDescription,
                itemType: item.ItemType?.TypeName ?? "Unknown",
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