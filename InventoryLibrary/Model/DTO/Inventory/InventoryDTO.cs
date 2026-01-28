public record InventoryDTO
(
    int id,
    string itemName,
    string? itemDescription,
    string itemType,
    int? ItemConditionId,
    double itemWeight,
    double itemPrice,
    DateTime addedDate,
    DateTime? warrantyEnd,
    DateTime lastInventoryDate,
    int? personInChargeId,
    string? room,
    int? stocktakeId
);