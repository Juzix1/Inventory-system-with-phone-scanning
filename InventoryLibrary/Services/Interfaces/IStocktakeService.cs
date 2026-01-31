using InventoryLibrary.Model.Accounts;
using InventoryLibrary.Model.DTO.Stocktake;
using InventoryLibrary.Model.Inventory;
using InventoryLibrary.Model.StockTake;

namespace InventoryLibrary.Services.Interfaces
{
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
        /// Pobiera inwentaryzacje dla konkretnego konta
        /// </summary>
        Task<IEnumerable<Stocktake>> GetStocktakesByAccount(int id);

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

        // ===== NOWE METODY DLA REFAKTORYZACJI =====
        
        /// <summary>
        /// Pobiera szczegółowe informacje o sprawdzonych przedmiotach (z datami i userami)
        /// </summary>
        Task<List<StocktakeCheckedItem>> GetCheckedItemsDetails(int stocktakeId);

        /// <summary>
        /// Sprawdza czy przedmiot jest oznaczony jako sprawdzony
        /// </summary>
        Task<bool> IsItemChecked(int stocktakeId, int itemId);

        /// <summary>
        /// Oznacza wiele przedmiotów jako sprawdzone (bulk operation)
        /// </summary>
        Task MarkMultipleItemsAsChecked(int stocktakeId, List<int> itemIds, string checkedBy = null);

        /// <summary>
        /// Usuwa oznaczenie wielu przedmiotów (bulk operation)
        /// </summary>
        Task UnmarkMultipleItemsAsChecked(int stocktakeId, List<int> itemIds);

        /// <summary>
        /// Pobiera liczbę sprawdzonych przedmiotów (zoptymalizowane zapytanie)
        /// </summary>
        Task<int> GetCheckedItemsCount(int stocktakeId);

        /// <summary>
        /// Pobiera procent postępu inwentaryzacji
        /// </summary>
        Task<int> GetProgressPercentage(int stocktakeId);
        Task<List<int>> GetAssignedItemsIds(int stocktakeId);
    }
}