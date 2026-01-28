import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';

class ScannerScreen extends StatefulWidget {
  final Function(String) onBarcodeScanned;
  
  const ScannerScreen({
    super.key, 
    required this.onBarcodeScanned,
  });

  @override
  State<ScannerScreen> createState() => _ScannerScreenState();
}

class _ScannerScreenState extends State<ScannerScreen> {
  bool _isScanned = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Scan Barcode'),
      ),
      body: MobileScanner(
        onDetect: (capture) {
          if (_isScanned) return;
          
          final List<Barcode> barcodes = capture.barcodes;
          for (final barcode in barcodes) {
            if (barcode.rawValue != null) {
              setState(() {
                _isScanned = true;
              });
              
              widget.onBarcodeScanned(barcode.rawValue!);
              Navigator.pop(context);
              break;
            }
          }
        },
      ),
    );
  }
}