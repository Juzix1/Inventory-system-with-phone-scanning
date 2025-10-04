using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConditionController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IConditionService _conditionService;

        public ConditionController(MyDbContext context, IConditionService conditionService)
        {
            _context = context;
            _conditionService = conditionService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<ItemCondition>> GetAllItemConditions() {
            var conditions = await _conditionService.GetAllItemConditions();
            if (conditions == null)
            {
                return NoContent();
            }
            return Ok(conditions);
        }
    }
}
