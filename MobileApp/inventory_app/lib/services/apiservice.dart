import 'dart:convert';
import 'dart:io';
import 'package:http/http.dart' as http;
import 'package:inventory_app/HttpOverride/NoCertHttpOverride.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:jwt_decoder/jwt_decoder.dart';

class ApiService {
  String? _baseUrl;
  String? _token;
  bool _initialized = false;

  ApiService() {
    HttpOverrides.global = NoCertHttpOverrides();
  }

  // Inicjalizacja - wywołaj przed użyciem
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

  Future<bool> isTokenValid() async {
    await initialize(); // Upewnij się że zainicjalizowano
    
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
    
    print('=== LOGIN DEBUG ===');
    print('URL: $url');
    print('Index: $index');
    
    try {
      final body = jsonEncode({
        'index': index,
        'password': password,
      });
      
      final response = await http.post(
        url,
        headers: {'Content-Type': 'application/json'},
        body: body,
      );

      print('Response status: ${response.statusCode}');
      print('Response body: ${response.body}');

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
        await prefs.setBool('isModerator', data['user']['isModerator'] ?? false);
        await prefs.setBool('resetPasswordOnNextLogin', 
            data['user']['resetPasswordOnNextLogin'] ?? false);
        
        DateTime expirationDate = JwtDecoder.getExpirationDate(data['token']);
        await prefs.setString('tokenExpiration', expirationDate.toIso8601String());
        
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
          resetPasswordOnNextLogin: data['user']['resetPasswordOnNextLogin'] ?? false,
        );
      } else {
        return LoginResponse(
          success: false,
          statusCode: response.statusCode,
          errorMessage: 'Login failed: ${response.body}',
        );
      }
    } catch (e, stackTrace) {
      print('Exception during login: $e');
      print('StackTrace: $stackTrace');
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