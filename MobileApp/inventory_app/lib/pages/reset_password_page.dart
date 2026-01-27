import 'package:flutter/material.dart';
import 'package:inventory_app/components/my_button.dart';
import 'package:inventory_app/components/my_textfield.dart';
import 'package:inventory_app/pages/home_page.dart';

class ResetPasswordPage extends StatelessWidget {
  final int userId;
  final TextEditingController _newPasswordController = TextEditingController();
  final TextEditingController _confirmPasswordController = TextEditingController();

  ResetPasswordPage({super.key, required this.userId});

  void resetPassword(BuildContext context) {
    if (_newPasswordController.text != _confirmPasswordController.text) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Passwords do not match')),
      );
      return;
    }

    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (context) => const HomePage()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Reset Password')),
      body: Padding(
        padding: const EdgeInsets.all(25.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Text('You must reset your password before continuing'),
            const SizedBox(height: 25),
            MyTextfield(
              hintText: "New Password",
              obscureText: true,
              controller: _newPasswordController,
            ),
            const SizedBox(height: 10),
            MyTextfield(
              hintText: "Confirm Password",
              obscureText: true,
              controller: _confirmPasswordController,
            ),
            const SizedBox(height: 25),
            MyButton(
              text: "Reset Password",
              onTap: () => resetPassword(context),
            ),
          ],
        ),
      ),
    );
  }
}