class Room {
  final int id;
  final String roomName;
  final int? departmentId;
  final String? departmentName;
  final String? departmentLocation;

  Room({
    required this.id,
    required this.roomName,
    this.departmentId,
    this.departmentName,
    this.departmentLocation,
  });

  factory Room.fromJson(Map<String, dynamic> json) {
    return Room(
      id: json['id'] as int,
      roomName: json['roomName'] as String,
      departmentId: json['departmentId'] as int?,
      departmentName: json['departmentName'] as String?,
      departmentLocation: json['departmentLocation'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'roomName': roomName,
      'departmentId': departmentId,
      'departmentName': departmentName,
      'departmentLocation': departmentLocation,
    };
  }

  @override
  String toString() {
    if (departmentName != null) {
      return '$roomName ($departmentName)';
    }
    return roomName;
  }

  String get displayName {
    if (departmentName != null && departmentLocation != null) {
      return '$roomName - $departmentName ($departmentLocation)';
    } else if (departmentName != null) {
      return '$roomName - $departmentName';
    }
    return roomName;
  }
}