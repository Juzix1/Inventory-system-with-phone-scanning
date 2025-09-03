import 'package:flutter/material.dart';
import 'package:inventory_app/components/scanner_screen.dart';
import 'package:inventory_app/pages/login_page.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  String? scannedBarcode;

  void _handleScannedBarcode(String barcode) {
    setState(() {
      scannedBarcode = barcode;
    });
    // Process the barcode here
    print('Scanned barcode: $barcode');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Center(
            child: Text(
              'Welcome to the Home Page!',
              style: TextStyle(
                fontSize: 24,
                color: Theme.of(context).colorScheme.primary,
              ),
            ),
          ),
          if (scannedBarcode != null)
            Text('Last scanned code: $scannedBarcode'),
          ElevatedButton(
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => LoginPage()),
              );
            },
            child: Text('Go Back'),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => ScannerScreen(
                onBarcodeScanned: _handleScannedBarcode,
              ),
            ),
          );
        },
        child: const Icon(Icons.qr_code_scanner),
      ),
    );
  }
}