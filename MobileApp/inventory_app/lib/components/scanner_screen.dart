import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';


class ScannerScreen extends StatelessWidget {
  final Function(String) onBarcodeScanned;
  const ScannerScreen({
    super.key, 
    required this.onBarcodeScanned,
    });


  

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Scan Barcode'),
      ),
      body: MobileScanner(
        onDetect: (capture) {
          final List<Barcode> barcodes = capture.barcodes;
          for (final barcode in barcodes) {
            if (barcode.rawValue != null) {
              onBarcodeScanned(barcode.rawValue!);
              Navigator.pop(context);
              break;
            }
          }
        },
      ),
    );
  }
}