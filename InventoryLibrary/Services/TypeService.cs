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
    private readonly IInventoryLogger<TypeService> _logger;

    public TypeService(MyDbContext context, IInventoryLogger<TypeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public ItemType CreateItemType(string name)
    {
        try
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
            _logger.LogInfo($"Created new ItemType with name: {name}");
            return type;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while creating new type", ex);
            return null;
        }
    }

    public async Task<List<ItemType>> GetAllTypesAsync()
    {
        try
        {
            if (!await _context.ItemTypes.AnyAsync())
            {
                return new List<ItemType>();
            }
            return await _context.ItemTypes.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while getting types", ex);
            return new List<ItemType>();
        }
    }

    public async void DeleteType(ItemType type)
    {
        try
        {
            _context.ItemTypes.Remove(type);
            await _context.SaveChangesAsync();
            _logger.LogWarning($"Deleted type with id: {type.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while deleting type", ex);
        }
    }

    public async void ChangeName(int index, string name)
    {
        try
        {
            var type = await _context.ItemTypes.FindAsync(index);
            if (type != null)
            {
                type.TypeName = name;
                await _context.SaveChangesAsync();
                _logger.LogInfo($"Changed name of type with id: {index} to {name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while changing name", ex);
        }
    }


}
