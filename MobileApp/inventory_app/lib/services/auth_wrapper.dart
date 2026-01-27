import 'dart:async';
import 'package:flutter/material.dart';
import 'package:inventory_app/pages/login_page.dart';
import 'package:inventory_app/services/apiservice.dart';

class AuthWrapper extends StatefulWidget {
  final Widget child;
  
  const AuthWrapper({super.key, required this.child});

  @override
  State<AuthWrapper> createState() => _AuthWrapperState();
}

class _AuthWrapperState extends State<AuthWrapper> {
  final ApiService _api = ApiService();
  Timer? _tokenCheckTimer;

  @override
  void initState() {
    super.initState();
    _startTokenCheck();
  }

  void _startTokenCheck() {
    // Sprawdzaj token co 30 sekund
    _tokenCheckTimer = Timer.periodic(const Duration(seconds: 30), (timer) async {
      final isValid = await _api.isTokenValid();
      
      if (!isValid && mounted) {
        // Token wygasł - wyloguj
        await _api.logout();
        
        // Pokaż komunikat
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Session expired. Please login again.'),
            backgroundColor: Colors.orange,
            duration: Duration(seconds: 3),
          ),
        );
        
        // Przekieruj na login
        Navigator.of(context).pushAndRemoveUntil(
          MaterialPageRoute(builder: (context) => const LoginPage()),
          (route) => false,
        );
      }
    });
  }

  @override
  void dispose() {
    _tokenCheckTimer?.cancel();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return widget.child;
  }
}