using InventoryLibrary.Data;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Services;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountsService _service;

        public AccountController(MyDbContext context, IAccountsService accountsService)
        {
            _service = new AccountsService(context, new PasswordService());
        }

        // [HttpPost("auth")]
        // public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        // {
        //     var auth = await _service.AuthenticateAsync(request.Id, request.Password);
        //     if (auth == null)
        //         return Unauthorized("Invalid credentials.");

        //     // TODO: Wygeneruj i zwróć JWT zamiast danych konta
        //     var token = "tst"; //GenerateJwtToken(auth);
        //     return Ok(new { Token = token });
        // }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount([FromBody] Account account)
        {
            if (account == null)
            {
                return BadRequest("Account data is null.");
            }
            try
            {
                var createdAccount = await _service.CreateAccountAsync(account);
                return CreatedAtAction(nameof(CreateAccount), new { id = createdAccount.Id }, createdAccount);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error creating account: {ex.Message}");
            }
        }

        [HttpGet("privilage-check/{id}")]
        public async Task<IActionResult> CheckPrivilageForApp(int id)
        {
            try
            {
                var hasPrivilage = await _service.CanAccessScanner(id);
                if(!hasPrivilage)
                {
                    return Forbid("Account does not have privilage to access the scanner app.");
                }
                return Ok(hasPrivilage);
                
            }catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error checking privilage: {ex.Message}");
            }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
