class ItemCondition {
  final int id;
  final String conditionName;

  ItemCondition({
    required this.id,
    required this.conditionName,
  });

  factory ItemCondition.fromJson(Map<String, dynamic> json) {
    return ItemCondition(
      id: json['id'] as int,
      conditionName: json['conditionName'] as String,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'conditionName': conditionName,
    };
  }

  @override
  String toString() => conditionName;
}