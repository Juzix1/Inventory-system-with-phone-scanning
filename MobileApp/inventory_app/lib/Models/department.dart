class Department {
  final int id;
  final String departmentName;
  final String departmentLocation;
  final int? roomCount;

  Department({
    required this.id,
    required this.departmentName,
    required this.departmentLocation,
    this.roomCount,
  });

  factory Department.fromJson(Map<String, dynamic> json) {
    return Department(
      id: json['id'] as int,
      departmentName: json['departmentName'] as String,
      departmentLocation: json['departmentLocation'] as String,
      roomCount: json['roomCount'] as int?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'departmentName': departmentName,
      'departmentLocation': departmentLocation,
      'roomCount': roomCount,
    };
  }

  @override
  String toString() => '$departmentName ($departmentLocation)';
}