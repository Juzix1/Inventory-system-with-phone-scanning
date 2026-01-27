import 'package:flutter/material.dart';
import 'package:inventory_app/pages/login_page.dart';
import 'package:inventory_app/services/apiservice.dart';
import 'package:inventory_app/services/auth_wrapper.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return AuthWrapper(
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Home'),
          actions: [
            IconButton(
              icon: const Icon(Icons.logout),
              onPressed: () async {
                final api = ApiService();
                await api.logout();
                Navigator.of(context).pushAndRemoveUntil(
                  MaterialPageRoute(builder: (context) => const LoginPage()),
                  (route) => false,
                );
              },
            ),
          ],
        ),
        body: const Center(
          child: Text('Welcome to Scanner App!'),
        ),
      ),
    );
  }
}