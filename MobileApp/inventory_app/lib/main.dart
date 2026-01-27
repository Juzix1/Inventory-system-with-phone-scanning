import 'package:flutter/material.dart';
import 'package:inventory_app/pages/login_page.dart';
import 'package:inventory_app/themes/light_mode.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Scanner App',
      theme: lightMode,
      home: const LoginPage(),
      debugShowCheckedModeBanner: false,
    );
  }
}