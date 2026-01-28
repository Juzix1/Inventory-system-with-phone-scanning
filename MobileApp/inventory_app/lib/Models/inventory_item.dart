class InventoryItem {
  final int id;
  final String itemName;
  final String? itemDescription;
  final String? itemType;
  final int? itemConditionId;
  final double itemWeight;
  final double itemPrice;
  final DateTime addedDate;
  final DateTime? warrantyEnd;
  final DateTime lastInventoryDate;
  final int? personInChargeId;
  final String? room;
  final int? stocktakeId;
  final String? imagePath;

  InventoryItem({
    required this.id,
    required this.itemName,
    this.itemDescription,
    required this.itemType,
    this.itemConditionId,
    required this.itemWeight,
    required this.itemPrice,
    required this.addedDate,
    this.warrantyEnd,
    required this.lastInventoryDate,
    this.personInChargeId,
    this.room,
    this.stocktakeId,
    this.imagePath
  });

  factory InventoryItem.fromJson(Map<String, dynamic> json) {
    return InventoryItem(
      id: json['id'] ?? 0,
      itemName: json['itemName'] ?? '',
      itemDescription: json['itemDescription'],
      itemType: json['itemType'],
      itemConditionId: json['itemConditionId'] ?? 0,
      itemWeight: (json['itemWeight'] ?? 0).toDouble(),
      itemPrice: (json['itemPrice'] ?? 0).toDouble(),
      addedDate: json['addedDate'] != null
          ? DateTime.parse(json['addedDate'])
          : DateTime.now(),
      warrantyEnd: json['warrantyEnd'] != null
          ? DateTime.parse(json['warrantyEnd'])
          : null,
      lastInventoryDate: json['lastInventoryDate'] != null
          ? DateTime.parse(json['lastInventoryDate'])
          : DateTime.now(),
      personInChargeId: json['personInChargeId'],
      room: json['room'],
      stocktakeId: json['stocktakeId'],
      imagePath: json['imagePath'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'itemName': itemName,
      'itemDescription': itemDescription,
      'itemType': itemType,
      'itemConditionId': itemConditionId,
      'itemWeight': itemWeight,
      'itemPrice': itemPrice,
      'addedDate': addedDate.toIso8601String(),
      'warrantyEnd': warrantyEnd?.toIso8601String(),
      'lastInventoryDate': lastInventoryDate.toIso8601String(),
      'personInChargeId': personInChargeId,
      'room': room,
      'stocktakeId': stocktakeId,
      'imagePath':imagePath,
    };
  }

  @override
  int get hashCode => id.hashCode;

  // Pomocnicze gettery - gwarancja
  bool get hasWarranty =>
      warrantyEnd != null && warrantyEnd!.isAfter(DateTime.now());

  bool get isWarrantyExpiringSoon {
    if (warrantyEnd == null) return false;
    final daysUntilExpiry = warrantyEnd!.difference(DateTime.now()).inDays;
    return daysUntilExpiry > 0 && daysUntilExpiry <= 30;
  }

  String get warrantyStatus {
    if (warrantyEnd == null) return 'No warranty';
    if (hasWarranty) {
      if (isWarrantyExpiringSoon) return 'Expiring soon';
      return 'Active';
    }
    return 'Expired';
  }

  // Formatowanie
  String get formattedPrice => '${itemPrice.toStringAsFixed(2)} PLN';

  String get formattedWeight => '${itemWeight.toStringAsFixed(2)} kg';

  String get formattedAddedDate {
    return '${addedDate.day}.${addedDate.month}.${addedDate.year}';
  }

  String get formattedWarrantyEnd {
    if (warrantyEnd == null) return 'No warranty';
    return '${warrantyEnd!.day}.${warrantyEnd!.month}.${warrantyEnd!.year}';
  }

  String get formattedLastInventoryDate {
    return '${lastInventoryDate.day}.${lastInventoryDate.month}.${lastInventoryDate.year}';
  }

  // Wiek przedmiotu
  int get ageInDays {
    return DateTime.now().difference(addedDate).inDays;
  }

  bool get isNewItem {
    return ageInDays <= 30;
  }

  int get daysSinceLastInventory {
    return DateTime.now().difference(lastInventoryDate).inDays;
  }

  bool get needsInventoryCheck {
    return daysSinceLastInventory > 90; // Przykład: ponad 90 dni
  }
}
