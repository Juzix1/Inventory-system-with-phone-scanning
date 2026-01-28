import 'package:flutter/material.dart';
import 'package:inventory_app/components/item_list.dart';
import 'package:inventory_app/components/scanner_screen.dart';
import 'package:inventory_app/pages/item_detail.page.dart';
import 'package:inventory_app/pages/login_page.dart';
import 'package:inventory_app/services/apiservice.dart';
import 'package:inventory_app/services/auth_wrapper.dart';

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  Future<void> _handleBarcodeScanned(
    BuildContext context,
    String barcode,
  ) async {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => const Center(child: CircularProgressIndicator()),
    );

    try {
      final itemId = int.parse(barcode);
      final item = await ApiService().getItemById(itemId);

      if (!context.mounted) return;

      // Zamknij loading indicator
      Navigator.pop(context);

      Navigator.push(
        context,
        MaterialPageRoute(builder: (context) => ItemDetailPage(item: item)),
      );
    } catch (e) {
      if (!context.mounted) return;

      Navigator.pop(context);

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Error: Item not found '),
          backgroundColor: Colors.red,
          duration: const Duration(seconds: 3),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return AuthWrapper(
      child: Scaffold(
        appBar: AppBar(
          backgroundColor: Theme.of(context).colorScheme.secondaryContainer,
          title: Row(
            children: [
              Icon(
                Icons.barcode_reader,
                color: Theme.of(context).colorScheme.surface,
              ),
              SizedBox(width: 15),
              Text(
                'Inventory Scanner',
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: Theme.of(context).colorScheme.surface,
                ),
              ),
            ],
          ),
          actions: [
            IconButton(
              icon: Icon(
                Icons.logout,
                color: Theme.of(context).colorScheme.surface,
              ),
              onPressed: () async {
                final shouldLogout = await showDialog<bool>(
                  context: context,
                  builder: (context) => AlertDialog(
                    title: const Text('Logout'),
                    content: const Text('Are you sure you want to logout?'),
                    actions: [
                      TextButton(
                        onPressed: () => Navigator.pop(context, false),
                        child: const Text('Cancel'),
                      ),
                      TextButton(
                        onPressed: () => Navigator.pop(context, true),
                        child: const Text('Logout'),
                      ),
                    ],
                  ),
                );
                if (shouldLogout == true) {
                  await ApiService().logout();
                  if (!context.mounted) return;
                  Navigator.of(context).pushAndRemoveUntil(
                    MaterialPageRoute(builder: (context) => const LoginPage()),
                    (route) => false,
                  );
                }
              },
            ),
          ],
        ),
        body: const ItemList(),
        backgroundColor: Theme.of(context).colorScheme.surface,
        floatingActionButton: FloatingActionButton.extended(
          onPressed: () {
            Navigator.push(
              context,
              MaterialPageRoute(
                builder: (context) => ScannerScreen(
                  onBarcodeScanned: (barcode) =>
                      _handleBarcodeScanned(context, barcode),
                ),
              ),
            );
          },
          icon: const Icon(Icons.barcode_reader),
          label: const Text('Scan'),
        ),
      ),
    );
  }
}
