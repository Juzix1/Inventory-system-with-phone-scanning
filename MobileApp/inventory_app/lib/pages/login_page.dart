import 'package:flutter/material.dart';
import 'package:inventory_app/components/my_button.dart';
import 'package:inventory_app/components/my_textfield.dart';
import 'package:inventory_app/pages/home_page.dart';
import 'package:inventory_app/pages/settings_page.dart';
import 'package:inventory_app/services/apiservice.dart';
import 'package:shared_preferences/shared_preferences.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({super.key});

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final TextEditingController _indexController = TextEditingController();
  final TextEditingController _pwController = TextEditingController();
  final ApiService _api = ApiService();
  bool _isLoading = false;
  bool _isCheckingAuth = true;

  @override
  void initState() {
    super.initState();
    _checkExistingLogin();
  }

  Future<void> _checkExistingLogin() async {
    try {
      await _api.initialize();
      
      final isLoggedIn = await _api.isLoggedIn();
      final isTokenValid = await _api.isTokenValid();

      print('Is logged in: $isLoggedIn');
      print('Is token valid: $isTokenValid');

      if (!mounted) return;

      if (isLoggedIn && isTokenValid) {
        // Token jest ważny - przejdź do home
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(builder: (context) => const HomePage()),
        );
      } else {
        // Token nieważny lub brak logowania
        if (isLoggedIn) {
          await _api.logout();
        }
        
        setState(() {
          _isCheckingAuth = false;
        });
      }
    } catch (e) {
      print('Error checking auth: $e');
      setState(() {
        _isCheckingAuth = false;
      });
    }
  }

  Future<void> login(BuildContext context) async {
  String indexStr = _indexController.text.trim();
  String password = _pwController.text;

  
  // Pobierz IP z ustawień
  await _api.initialize();
  
  // Sprawdź bezpośrednio w SharedPreferences
  final prefs = await SharedPreferences.getInstance();
  final savedIp = prefs.getString('baseUrl');
  print('IP from SharedPreferences: $savedIp');
  
  final userData = await _api.getUserData();
  print('User data: $userData');
  String? ip = userData?['baseUrl'];
  print('IP from getUserData: $ip');

  if (ip == null || ip.isEmpty) {
    // Spróbuj użyć bezpośrednio z SharedPreferences
    if (savedIp != null && savedIp.isNotEmpty) {
      ip = savedIp;
      print('Using IP from SharedPreferences: $ip');
    } else {
      _showError('Please set IP address in settings first');
      return;
    }
  }

  if (indexStr.isEmpty || password.isEmpty) {
    _showError('Please fill all fields');
    return;
  }

  int? index = int.tryParse(indexStr);
  if (index == null || index <= 0) {
    _showError('Please enter a valid index number');
    return;
  }

  setState(() {
    _isLoading = true;
  });

  try {
    print('Attempting login with IP: $ip, Index: $index');
    final response = await _api.login(ip, index, password);

    if (!mounted) return;

    setState(() {
      _isLoading = false;
    });

    if (response.success) {
      if (response.resetPasswordOnNextLogin == true) {
        _showError('You must reset your password');
      } else {
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(builder: (context) => const HomePage()),
        );
      }
    } else {
      _showError(response.errorMessage ?? 'Login failed');
    }
  } catch (e) {
    if (!mounted) return;
    
    setState(() {
      _isLoading = false;
    });
    _showError('Error: $e');
  }
}

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: Colors.red,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_isCheckingAuth) {
      return Scaffold(
        backgroundColor: Theme.of(context).colorScheme.surface,
        body: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(
                Icons.barcode_reader,
                size: 100,
                color: Theme.of(context).colorScheme.surface,
              ),
              const SizedBox(height: 20),
              Text(
                "Scanner App",
                style: TextStyle(
                  fontSize: 40,
                  fontWeight: FontWeight.bold,
                  color: Theme.of(context).colorScheme.surface,
                ),
              ),
              const SizedBox(height: 30),
              CircularProgressIndicator(
                color: Theme.of(context).colorScheme.surface,
              ),
              const SizedBox(height: 20),
              Text(
                'Checking authentication...',
                style: TextStyle(
                  fontSize: 14,
                  color: Theme.of(context).colorScheme.onPrimaryContainer,
                ),
              ),
            ],
          ),
        ),
      );
    }

    return Scaffold(
      backgroundColor: Theme.of(context).colorScheme.surface,
      body: Stack(
        children: [
          Positioned(
            top: 40,
            right: 20,
            child: IconButton(
              icon: Icon(
                Icons.settings,
                size: 30,
                color: Theme.of(context).colorScheme.secondaryContainer,
              ),
              onPressed: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(builder: (context) => const SettingsPage()),
                );
              },
            ),
          ),
          
          Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(25.0),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.barcode_reader,
                    size: 60,
                    color: Theme.of(context).colorScheme.secondaryContainer,
                  ),
                  const SizedBox(height: 5),
                  Text(
                    "Scanner App",
                    style: TextStyle(
                      fontSize: 35,
                      color: Theme.of(context).colorScheme.secondaryContainer,
                      fontWeight: FontWeight.bold
                    ),
                  ),
                  const SizedBox(height: 50),

                  MyTextfield(
                    hintText: "Index number",
                    obscureText: false,
                    controller: _indexController,
                  ),
                  const SizedBox(height: 10),

                  MyTextfield(
                    hintText: "Password",
                    obscureText: true,
                    controller: _pwController,
                  ),
                  const SizedBox(height: 25),

                  _isLoading
                      ? const CircularProgressIndicator()
                      : MyButton(
                          text: "Login",
                          onTap: () => login(context),
                        ),
                  const SizedBox(height: 25),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _indexController.dispose();
    _pwController.dispose();
    super.dispose();
  }
}