using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ITypeService _service;
        private readonly IInventoryService _inventory;

        public TestController(MyDbContext context, ITypeService typeService, IInventoryService inventory)
        {
            _context = context;
            _service = typeService;
            _inventory = inventory;
        }

        [HttpPost("{name}")]
        public async Task<ActionResult> CreateNewType(string name)
        {
            try
            {
                var type = _service.CreateItemType(name);
                return Ok(type);
            }
            catch (Exception se)
            {
                Console.WriteLine($"{se.Message}");
                return BadRequest();
            }

        }

        [HttpGet]
        public async Task<ActionResult> GetAllTypes()
        {
            var types = await _service.GetAllTypes();
            return Ok(types);
        }
    }
}
