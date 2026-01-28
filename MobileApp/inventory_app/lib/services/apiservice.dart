import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:inventory_app/HttpOverride/NoCertHttpOverride.dart';
import 'package:inventory_app/Models/inventory_item.dart';
import 'package:inventory_app/Models/item_condition.dart';
import 'package:inventory_app/Models/room.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:jwt_decoder/jwt_decoder.dart';

class ApiService {
  String? _baseUrl;
  String? _token;
  bool _initialized = false;

  ApiService() {
    HttpOverrides.global = NoCertHttpOverrides();
  }

  Future<void> initialize() async {
    if (_initialized) return;

    final prefs = await SharedPreferences.getInstance();
    _token = prefs.getString('token');

    final savedBaseUrl = prefs.getString('baseUrl');
    if (savedBaseUrl != null) {
      setBaseUrl(savedBaseUrl);
    }

    _initialized = true;
  }

  void setBaseUrl(String ip) {
    if (!ip.startsWith('http://') && !ip.startsWith('https://')) {
      _baseUrl = 'https://$ip/api';
    } else {
      _baseUrl = '$ip/api';
    }
  }

  // Helper method to build full image URL
  String? getImageUrl(String? imagePath) {
    if (imagePath == null || imagePath.isEmpty || _baseUrl == null) {
      return null;
    }
    return '$_baseUrl/images/$imagePath';
  }

  // Get base URL for items to use
  String? get baseUrl => _baseUrl;

  Future<bool> isTokenValid() async {
    await initialize();

    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');

    if (token == null) return false;

    try {
      bool isExpired = JwtDecoder.isExpired(token);
      return !isExpired;
    } catch (e) {
      return false;
    }
  }

  Future<int?> getTokenExpirationTime() async {
    await initialize();

    final prefs = await SharedPreferences.getInstance();
    final token = prefs.getString('token');

    if (token == null) return null;

    try {
      DateTime expirationDate = JwtDecoder.getExpirationDate(token);
      Duration timeUntilExpiration = expirationDate.difference(DateTime.now());
      return timeUntilExpiration.inSeconds;
    } catch (e) {
      return null;
    }
  }

  Map<String, String> _getHeaders() {
    final headers = {'Content-Type': 'application/json'};
    if (_token != null) {
      headers['Authorization'] = 'Bearer $_token';
    }
    return headers;
  }

  Future<LoginResponse> login(String ip, int index, String password) async {
    setBaseUrl(ip);

    final url = Uri.parse('$_baseUrl/Account/login');

    try {
      final body = jsonEncode({'index': index, 'password': password});

      final response = await http.post(
        url,
        headers: {'Content-Type': 'application/json'},
        body: body,
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);

        final prefs = await SharedPreferences.getInstance();
        await prefs.setString('token', data['token']);
        await prefs.setString('baseUrl', ip);
        await prefs.setInt('userId', data['user']['id']);
        await prefs.setString('userEmail', data['user']['email'] ?? '');
        await prefs.setString('userName', data['user']['name'] ?? '');
        await prefs.setString('userRole', data['user']['role'] ?? '');
        await prefs.setBool('isAdmin', data['user']['isAdmin'] ?? false);
        await prefs.setBool(
          'isModerator',
          data['user']['isModerator'] ?? false,
        );
        await prefs.setBool(
          'resetPasswordOnNextLogin',
          data['user']['resetPasswordOnNextLogin'] ?? false,
        );

        DateTime expirationDate = JwtDecoder.getExpirationDate(data['token']);
        await prefs.setString(
          'tokenExpiration',
          expirationDate.toIso8601String(),
        );

        _token = data['token'];
        _baseUrl = 'https://$ip/api';

        return LoginResponse(
          success: true,
          statusCode: response.statusCode,
          token: data['token'],
          expiresIn: data['expiresIn'],
          userId: data['user']['id'],
          email: data['user']['email'] ?? '',
          name: data['user']['name'] ?? '',
          role: data['user']['role'] ?? '',
          isAdmin: data['user']['isAdmin'] ?? false,
          isModerator: data['user']['isModerator'] ?? false,
          resetPasswordOnNextLogin:
              data['user']['resetPasswordOnNextLogin'] ?? false,
        );
      } else {
        return LoginResponse(
          success: false,
          statusCode: response.statusCode,
          errorMessage: 'Login failed: ${response.body}',
        );
      }
    } catch (e, stackTrace) {
      return LoginResponse(
        success: false,
        statusCode: 0,
        errorMessage: 'Connection error: $e',
      );
    }
  }

  Future<bool> checkPrivilege(int userId) async {
    await initialize();

    final url = Uri.parse('$_baseUrl/Account/privilage-check/$userId');

    try {
      final response = await http.get(url, headers: _getHeaders());
      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();

    final savedIp = prefs.getString('baseUrl');
    await prefs.clear();
    if (savedIp != null) {
      await prefs.setString('baseUrl', savedIp);
    }

    _token = null;
    _baseUrl = null;
    _initialized = false;
  }

  Future<bool> isLoggedIn() async {
    await initialize();

    final prefs = await SharedPreferences.getInstance();
    final hasToken = prefs.containsKey('token');

    if (!hasToken) return false;

    return await isTokenValid();
  }

  Future<InventoryItem> getItemById(int id) async {
    await initialize();
    try {
      final url = '$_baseUrl/Inventory/$id';
      print('Fetching from URL: $url');

      final response = await http.get(
        Uri.parse(url),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $_token',
        },
      );

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        return InventoryItem.fromJson(json);
      } else if (response.statusCode == 404) {
        throw Exception('Item with ID $id not found');
      } else if (response.statusCode == 401) {
        throw Exception('Unauthorized - please login again');
      } else {
        throw Exception(
          'Server error ${response.statusCode}: ${response.body}',
        );
      }
    } catch (e) {
      rethrow;
    }
  }

  Future<List<InventoryItem>> getMyItems() async {
    await initialize();

    final url = Uri.parse('$_baseUrl/Stocktake/my-items');

    try {
      final response = await http.get(url, headers: _getHeaders());

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        final items = data.map((json) => InventoryItem.fromJson(json)).toList();

        return items;
      } else if (response.statusCode == 401) {
        throw Exception('Unauthorized - please login again');
      } else if (response.statusCode == 404) {
        throw Exception('User not found');
      } else {
        throw Exception('Failed to load my items: ${response.statusCode}');
      }
    } catch (e, stackTrace) {
      throw Exception('Error loading my items: $e');
    }
  }

  Future<Map<String, dynamic>?> getUserData() async {
    await initialize();

    final prefs = await SharedPreferences.getInstance();

    if (!prefs.containsKey('userId')) {
      return null;
    }

    return {
      'userId': prefs.getInt('userId'),
      'email': prefs.getString('userEmail'),
      'name': prefs.getString('userName'),
      'role': prefs.getString('userRole'),
      'isAdmin': prefs.getBool('isAdmin'),
      'isModerator': prefs.getBool('isModerator'),
      'baseUrl': prefs.getString('baseUrl'),
    };
  }

  // NEW METHODS FOR CONDITIONS AND ROOMS

  /// Get all available item conditions
  Future<List<ItemCondition>?> getAllConditions() async {
    await initialize();

    final url = Uri.parse('$_baseUrl/Condition/all');

    try {
      final response = await http.get(url, headers: _getHeaders());

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => ItemCondition.fromJson(json)).toList();
      } else if (response.statusCode == 204) {
        // No content
        return [];
      } else if (response.statusCode == 401) {
        throw Exception('Unauthorized - please login again');
      } else {
        throw Exception('Failed to load conditions: ${response.statusCode}');
      }
    } catch (e) {
      print('Error loading conditions: $e');
      return null;
    }
  }

  /// Get all available rooms
  /// Note: You'll need to create a similar endpoint in your backend
  Future<List<Room>?> getAllRooms() async {
    await initialize();

    final url = Uri.parse(
      '$_baseUrl/Room/all',
    ); // You need to create this endpoint

    try {
      final response = await http.get(url, headers: _getHeaders());

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        return data.map((json) => Room.fromJson(json)).toList();
      } else if (response.statusCode == 204) {
        return [];
      } else if (response.statusCode == 401) {
        throw Exception('Unauthorized - please login again');
      } else {
        throw Exception('Failed to load rooms: ${response.statusCode}');
      }
    } catch (e) {
      print('Error loading rooms: $e');
      return null;
    }
  }

  /// Update item condition
  Future<InventoryItem> updateItemCondition(int itemId, int conditionId) async {
    await initialize();

    // First, get the current item
    final currentItem = await getItemById(itemId);

    // Update the condition
    final url = Uri.parse('$_baseUrl/Inventory/$itemId/condition/$conditionId');
    try {
      // Create updated item object
      final updatedItemData = {
        'id': currentItem.id,
        'itemName': currentItem.itemName,
        'itemDescription': currentItem.itemDescription,
        'itemType': currentItem.itemType,
        'price': currentItem.itemPrice,
        'weight': currentItem.itemWeight,
        'itemConditionId': conditionId,
        'roomId': currentItem.room != null ? null : null,
        'personInChargeId': currentItem.personInChargeId,
        'warrantyEnd': currentItem.warrantyEnd?.toIso8601String(),
        'imagePath': currentItem.imagePath,
      };

      final response = await http.patch(url, headers: _getHeaders());

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        return InventoryItem.fromJson(json);
      } else if (response.statusCode == 404) {
        throw Exception('Item not found');
      } else if (response.statusCode == 401) {
        throw Exception('Unauthorized - please login again');
      } else {
        throw Exception(
          'Failed to update item: ${response.statusCode} - ${response.body}',
        );
      }
    } catch (e) {
      print('Error updating item condition: $e');
      rethrow;
    }
  }

  /// Update item location (room)
  Future<InventoryItem> updateItemLocation(int itemId, int roomId) async {
    await initialize();

    // First, get the current item
    final currentItem = await getItemById(itemId);

    // Update the location
    final url = Uri.parse('$_baseUrl/Inventory/$itemId/location/$roomId');
    try {
      // Create updated item object
      final updatedItemData = {
        'id': currentItem.id,
        'itemName': currentItem.itemName,
        'itemDescription': currentItem.itemDescription,
        'itemType': currentItem.itemType,
        'price': currentItem.itemPrice,
        'weight': currentItem.itemWeight,
        'itemConditionId': currentItem.itemConditionId,
        'roomId': roomId, // Updated field
        'personInChargeId':
            null, // Set to null when assigning to room (per backend logic)
        'warrantyEnd': currentItem.warrantyEnd?.toIso8601String(),
        'imagePath': currentItem.imagePath,
      };

      final response = await http.patch(url, headers: _getHeaders());

      if (response.statusCode == 200) {
        final json = jsonDecode(response.body);
        return InventoryItem.fromJson(json);
      } else if (response.statusCode == 404) {
        throw Exception('Item not found');
      } else if (response.statusCode == 401) {
        throw Exception('Unauthorized - please login again');
      } else {
        throw Exception(
          'Failed to update item: ${response.statusCode} - ${response.body}',
        );
      }
    } catch (e) {
      print('Error updating item location: $e');
      rethrow;
    }
  }
}

class LoginResponse {
  final bool success;
  final int statusCode;
  final String? token;
  final int? expiresIn;
  final int? userId;
  final String? name;
  final String? email;
  final String? role;
  final bool? isAdmin;
  final bool? isModerator;
  final bool? resetPasswordOnNextLogin;
  final String? errorMessage;

  LoginResponse({
    required this.success,
    required this.statusCode,
    this.token,
    this.expiresIn,
    this.userId,
    this.name,
    this.email,
    this.role,
    this.isAdmin,
    this.isModerator,
    this.resetPasswordOnNextLogin,
    this.errorMessage,
  });
}
