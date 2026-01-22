using System;
using System.Security.Cryptography;
using InventoryLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace InventoryLibrary.Services;

public class PasswordService:IPasswordService
{
    private const int SaltSize = 128 / 8;
    private const int KeySize = 256 / 8;
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
    private const char Delimiter = ';';

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize
        );

        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string hashedPassword, string inputPassword)
    {

        if(string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(inputPassword))
        {
            return false;
        }
        var parts = hashedPassword.Split(Delimiter);
        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var hashInput = Rfc2898DeriveBytes.Pbkdf2(
            inputPassword,
            salt,
            Iterations,
            HashAlgorithm,
            KeySize
        );

        return CryptographicOperations.FixedTimeEquals(hash, hashInput);
    }
}
