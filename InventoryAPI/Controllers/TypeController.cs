using InventoryLibrary.Data;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ITypeService _service;

        public TypeController(MyDbContext context, ITypeService typeService)
        {
            _context = context;
            _service = typeService;
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

        [HttpGet("all")]
        public async Task<ActionResult> GetAllTypes()
        {
            var types = await _service.GetAllTypesAsync();
            return Ok(types);
        }
    }
}
