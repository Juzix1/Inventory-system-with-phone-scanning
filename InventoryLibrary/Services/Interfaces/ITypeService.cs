using System;
using InventoryLibrary.Model.Inventory;

namespace InventoryLibrary.Services.Interfaces;

public interface ITypeService
{
    ItemType CreateItemType(string name);
    Task<List<ItemType>> GetAllTypesAsync();
    void DeleteType(ItemType type);
    void ChangeName(int index, string name);
    
}
