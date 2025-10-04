using System;
using System.Reflection.Metadata;
using InventoryLibrary.Data;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InventoryLibrary.Services;

public class TypeService : ITypeService
{

    private readonly MyDbContext _context;

    public TypeService(MyDbContext context)
    {
        _context = context;
    }

    public ItemType CreateItemType(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var type = new ItemType()
        {
            TypeName = name,

        };

        _context.ItemTypes.Add(type);
        _context.SaveChanges();
        return type;
    }

    public async Task<List<ItemType>> GetAllTypes()
    {
        if (!await _context.ItemTypes.AnyAsync())
        {
            return new List<ItemType>();
        }
        // return await _context.ItemTypes.ToListAsync();
        return await _context.ItemTypes.ToListAsync();
    }

    public async void DeleteType(ItemType type)
    {
        _context.ItemTypes.Remove(type);
        _context.SaveChanges();
    }
}
