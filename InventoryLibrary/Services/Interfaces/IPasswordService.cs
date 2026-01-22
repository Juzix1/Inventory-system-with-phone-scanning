using System;

namespace InventoryLibrary.Services.Interfaces;

public interface IPasswordService
{
    bool VerifyPassword(string hashedPassword, string inputPassword);
    string Hash(string password);

}
