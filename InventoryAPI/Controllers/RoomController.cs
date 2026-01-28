using InventoryLibrary.Data;
using InventoryLibrary.Model.DTO.Location;
using InventoryLibrary.Model.Location;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly MyDbContext _context;

        public RoomController(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all rooms with their department information
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<RoomDTO>>> GetAllRooms()
        {
            try
            {
                var rooms = await _context.Rooms
                    .Include(r => r.Department)
                    .Select(r => new RoomDTO
                    {
                        id = r.Id,
                        roomName = r.RoomName,
                        departmentId = r.DepartmentId,
                        departmentName = r.Department != null
                            ? r.Department.DepartmentName
                            : null,
                        departmentLocation = r.Department != null
                            ? r.Department.DepartmentLocation
                            : null
                    })
                    .OrderBy(r => r.departmentName)
                    .ThenBy(r => r.roomName)
                    .ToListAsync();


                if (rooms == null || !rooms.Any())
                {
                    return NoContent();
                }

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving rooms", error = ex.Message });
            }
        }

        /// <summary>
        /// Get room by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDTO>> GetRoomById(int id)
        {
            try
            {
                var room = await _context.Rooms
                    .Include(r => r.Department)
                    .Where(r => r.Id == id)
                    .Select(r => new RoomDTO
                    {
                        id = r.Id,
                        roomName = r.RoomName,
                        departmentId = r.DepartmentId,
                        departmentName = r.Department != null
                            ? r.Department.DepartmentName
                            : null,
                        departmentLocation = r.Department != null
                            ? r.Department.DepartmentLocation
                            : null
                    })
                    .FirstOrDefaultAsync();

                if (room == null)
                {
                    return NotFound(new { message = $"Room with ID {id} not found" });
                }

                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving room", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all rooms in a specific department
        /// </summary>
        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<RoomDTO>>> GetRoomsByDepartment(int departmentId)
        {
            try
            {
                var rooms = await _context.Rooms
                    .Include(r => r.Department)
                    .Where(r => r.DepartmentId == departmentId)
                    .Select(r => new RoomDTO
                    {
                        id = r.Id,
                        roomName = r.RoomName,
                        departmentId = r.DepartmentId,
                        departmentName = r.Department.DepartmentName,
                        departmentLocation = r.Department.DepartmentLocation
                    })
        .OrderBy(r => r.roomName)
        .ToListAsync();

                if (rooms == null || !rooms.Any())
                {
                    return NoContent();
                }

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving rooms", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all departments
        /// </summary>
        [HttpGet("departments")]
        public async Task<ActionResult<IEnumerable<DepartmentDTO>>> GetAllDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .Select(d => new DepartmentDTO
                    {
                        id = d.Id,
                        departmentName = d.DepartmentName,
                        departmentLocation = d.DepartmentLocation,
                        roomCount = d.Rooms.Count()
                    })
                    .OrderBy(d => d.departmentName)
                    .ToListAsync();

                if (departments == null || !departments.Any())
                {
                    return NoContent();
                }

                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving departments", error = ex.Message });
            }
        }
    }
}