using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;

namespace InventoryLibrary.Services.Interfaces;

public interface IStocktakeService
{
    /// <summary>
    /// Tworzy nową inwentaryzację z pełnym zestawem parametrów
    /// </summary>
    Task CreateNewStocktake(
        string name,
        string description,
        List<InventoryItem> items,
        List<Account> authorizedAccounts,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Tworzy nową inwentaryzację (stara wersja dla kompatybilności)
    /// </summary>
    Task CreateNewStocktake(
        List<InventoryItem> items,
        DateTime? startDate,
        DateTime endDate);

    /// <summary>
    /// Pobiera wszystkie inwentaryzacje
    /// </summary>
    Task<IEnumerable<Stocktake>> GetAllStocktakesAsync();

    /// <summary>
    /// Pobiera inwentaryzację po ID
    /// </summary>
    Task<Stocktake> GetStocktakeById(int id);

    /// <summary>
    /// Aktualizuje istniejącą inwentaryzację
    /// </summary>
    Task<Stocktake> UpdateStocktake(Stocktake stocktake);

    /// <summary>
    /// Oznacza przedmiot jako sprawdzony w inwentaryzacji
    /// </summary>
    Task MarkItemAsChecked(int stocktakeId, int itemId, string checkedBy = null);

    /// <summary>
    /// Stara nazwa metody dla kompatybilności
    /// </summary>
    Task MarkItem(int id, InventoryItem item);

    /// <summary>
    /// Usuwa oznaczenie przedmiotu jako sprawdzonego
    /// </summary>
    Task UnmarkItemAsChecked(int stocktakeId, int itemId);

    /// <summary>
    /// Usuwa inwentaryzację
    /// </summary>
    Task<Stocktake> DeleteStocktakeAsync(int id);

    /// <summary>
    /// Pobiera statystyki dla inwentaryzacji
    /// </summary>
    Task<StocktakeStatistics> GetStocktakeStatistics(int stocktakeId);

    /// <summary>
    /// Pobiera listę niesprawdzonych przedmiotów
    /// </summary>
    Task<List<InventoryItem>> GetUncheckedItems(int stocktakeId);

    /// <summary>
    /// Sprawdza czy użytkownik jest upoważniony do inwentaryzacji
    /// </summary>
    Task<bool> IsUserAuthorized(int stocktakeId, int userId);
}