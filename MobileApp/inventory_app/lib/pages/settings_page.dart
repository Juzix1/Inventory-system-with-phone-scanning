import 'package:flutter/material.dart';
import 'package:inventory_app/components/my_button.dart';
import 'package:inventory_app/components/my_textfield.dart';
import 'package:shared_preferences/shared_preferences.dart';

class SettingsPage extends StatefulWidget {
  const SettingsPage({super.key});

  @override
  State<SettingsPage> createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage> {
  final TextEditingController _ipController = TextEditingController();
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadCurrentIp();
  }

  Future<void> _loadCurrentIp() async {
    final prefs = await SharedPreferences.getInstance();
    final savedIp = prefs.getString('baseUrl');
    if (savedIp != null) {
      setState(() {
        _ipController.text = savedIp;
      });
    }
  }

  Future<void> _saveIp() async {
    if (_ipController.text.trim().isEmpty) {
      _showMessage('Please enter IP address', isError: true);
      return;
    }

    setState(() {
      _isLoading = true;
    });

    try {
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('baseUrl', _ipController.text.trim());
      
      setState(() {
        _isLoading = false;
      });

      _showMessage('IP address saved successfully!', isError: false);
    } catch (e) {
      setState(() {
        _isLoading = false;
      });
      _showMessage('Error saving IP: $e', isError: true);
    }
  }

  void _showMessage(String message, {required bool isError}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? Colors.red : Colors.green,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Settings'),
      ),
      body: Padding(
        padding: const EdgeInsets.all(25.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Server Configuration',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.bold,
                color: Theme.of(context).colorScheme.primary,
              ),
            ),
            const SizedBox(height: 20),
            MyTextfield(
              hintText: "IP Address and Port",
              obscureText: false,
              controller: _ipController,
            ),
            const SizedBox(height: 10),
            Text(
              'Enter the IP address and port of your server',
              style: TextStyle(
                fontSize: 12,
                color: Colors.grey[600],
              ),
            ),
            const SizedBox(height: 25),
            _isLoading
                ? const Center(child: CircularProgressIndicator())
                : MyButton(
                    text: "Save",
                    onTap: _saveIp,
                  ),
          ],
        ),
      ),
    );
  }

  @override
  void dispose() {
    _ipController.dispose();
    super.dispose();
  }
}