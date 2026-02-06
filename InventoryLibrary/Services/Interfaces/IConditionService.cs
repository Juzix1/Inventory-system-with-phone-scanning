using System;
using InventoryLibrary.Model.Inventory;

namespace InventoryLibrary.Services.Interfaces;

public interface IConditionService
{
    Task<IEnumerable<ItemCondition>> GetAllItemConditions();
}
