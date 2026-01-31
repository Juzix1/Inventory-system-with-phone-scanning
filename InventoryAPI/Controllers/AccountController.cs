using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountsService _service;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AccountController(IAccountsService accountsService, IJwtService jwtService, IConfiguration configuration)
        {
            _service = accountsService;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {                
                if (request.Index == 0 || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Index and password are required." });
                }

                var authenticatedAccount = await _service.AuthenticateAsync(request.Index, request.Password);
                if (authenticatedAccount == null)
                {
                    return Unauthorized(new { message = "Invalid credentials." });
                }
                var token = _jwtService.GenerateToken(
                    authenticatedAccount.Id,
                    authenticatedAccount.Email,
                    authenticatedAccount.Role,
                    authenticatedAccount.IsAdmin
                );
                var response = new
                {
                    Token = token,
                    ExpiresIn = _configuration.GetValue<int>("JwtSettings:ExpirationInMinutes") * 60,
                    User = new
                    {
                        Id = authenticatedAccount.Id,
                        Name = authenticatedAccount.Name ?? "",
                        Email = authenticatedAccount.Email ?? "",
                        Role = authenticatedAccount.Role ?? "",
                        IsAdmin = authenticatedAccount.IsAdmin,
                        IsModerator = authenticatedAccount.IsModerator,
                        ResetPasswordOnNextLogin = authenticatedAccount.resetPasswordOnNextLogin
                    }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = $"Error during login: {ex.Message}" });
            }
        }
        public class LoginRequest
        {
            public int Index { get; set; }
            public string Password { get; set; }
        }
    }
}