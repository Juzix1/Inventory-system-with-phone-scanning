using System;
using InventoryLibrary.Model.Inventory;

namespace InventoryLibrary.Services.Interfaces;

public interface ITypeService
{
    ItemType CreateItemType(string name);
    Task<List<ItemType>> GetAllTypes();
    void DeleteType(ItemType type);
}
