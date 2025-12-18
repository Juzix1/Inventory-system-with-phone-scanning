using System;
using System.Text.Json.Serialization;
using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.Inventory;

namespace InventoryLibrary.Model.StockTake;

public class Stocktake
{
    public int Id {get;set;}

    public string? Name {get;set;}
    public string? Description{get;set;}

    public DateTime CreatedDate {get;set;}
    public DateTime StartDate {get;set;}
    public DateTime EndDate {get;set;}
    public int AllItems {get;set;}

    public StockTakeStatus Status {get;set;} = StockTakeStatus.Planned;

    [JsonIgnore]
    public List<InventoryItem> ItemsToCheck {get;set;} = [];
    public List<int> CheckedItemIdList{get;set;} = [];
    public List<Account> AuthorizedAccounts {get;set;} = [];


}

public enum StockTakeStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled
}
