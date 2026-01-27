using System.Security.Claims;

public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role, bool isAdmin);
        ClaimsPrincipal? ValidateToken(string token);
    }