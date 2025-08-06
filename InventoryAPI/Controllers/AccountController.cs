using InventoryAPI.Data;
using InventoryAPI.Model.Account;
using InventoryAPI.Services;
using InventoryAPI.Services.Interfaces;
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
            _service = new AccountsService(context);
        }

        [HttpPost("auth")]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        {
            var auth = await _service.AuthenticateAsync(request.Email, request.Password);
            if (auth == null)
                return Unauthorized("Invalid credentials.");

            // TODO: Wygeneruj i zwróć JWT zamiast danych konta
            var token = "tst"; //GenerateJwtToken(auth);
            return Ok(new { Token = token });
        }

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

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
