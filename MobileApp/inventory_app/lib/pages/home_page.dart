import 'package:flutter/material.dart';
import 'package:inventory_app/Models/inventory_item.dart';
import 'package:inventory_app/components/item_list.dart';
import 'package:inventory_app/components/scanner_screen.dart';
import 'package:inventory_app/pages/item_detail.page.dart';
import 'package:inventory_app/pages/login_page.dart';
import 'package:inventory_app/services/apiservice.dart';
import 'package:inventory_app/services/auth_wrapper.dart';

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  final ApiService _api = ApiService();
  List<InventoryItem>? _myItemsCache;
  DateTime? _cacheTime;

  Future<List<InventoryItem>> _getMyItems() async {
    // Cache na 2 minuty (krótszy bo status się zmienia)
    if (_myItemsCache != null && 
        _cacheTime != null && 
        DateTime.now().difference(_cacheTime!) < const Duration(minutes: 2)) {
      return _myItemsCache!;
    }

    _myItemsCache = await _api.getMyItems();
    _cacheTime = DateTime.now();
    return _myItemsCache!;
  }

  void _invalidateCache() {
    setState(() {
      _myItemsCache = null;
      _cacheTime = null;
    });
  }

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
      final item = await _api.getItemById(itemId);

      // Sprawdź czy item należy do aktywnej inwentaryzacji użytkownika
      final myItems = await _getMyItems();
      final myItem = myItems.firstWhere(
        (myItem) => myItem.id == itemId,
        orElse: () => throw Exception('Not your item'),
      );

      if (!context.mounted) return;
      Navigator.pop(context); // Zamknij loading

      if (myItem.stocktakeId != null) {
        // Pobierz status stocktake żeby sprawdzić czy już oznaczony
        try {
          final progress = await _api.getStocktakeProgress(myItem.stocktakeId!);
          final checkedItemIds = List<int>.from(progress['checkedItemIds'] ?? []);
          final isAlreadyChecked = checkedItemIds.contains(itemId);

          if (isAlreadyChecked) {
            // Item już został sprawdzony
            if (!context.mounted) return;
            
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Row(
                  children: [
                    const Icon(Icons.info, color: Colors.white),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        'Item already checked! Progress: ${progress['progress']}',
                      ),
                    ),
                  ],
                ),
                backgroundColor: Colors.blue,
                duration: const Duration(seconds: 3),
              ),
            );
          } else {
            // Item jeszcze nie sprawdzony - oznacz go
            final result = await _api.markItemAsChecked(
              myItem.stocktakeId!,
              itemId,
            );

            // Invalidate cache żeby odświeżyć listę
            _invalidateCache();

            if (!context.mounted) return;

            // Pokaż sukces z progress
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Row(
                  children: [
                    const Icon(Icons.check_circle, color: Colors.white),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        'Item marked as checked! ${result['progress']}',
                      ),
                    ),
                  ],
                ),
                backgroundColor: Colors.green,
                duration: const Duration(seconds: 3),
              ),
            );

            // Jeśli inwentaryzacja się zakończyła
            if (result['isCompleted'] == true) {
              _showCompletionDialog(context, result);
            }
          }
        } catch (e) {
          if (!context.mounted) return;
          
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Error checking item status: $e'),
              backgroundColor: Colors.orange,
              duration: const Duration(seconds: 3),
            ),
          );
        }
      } else {
        // Item nie ma przypisanej inwentaryzacji
        if (!context.mounted) return;
        
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: const Row(
              children: [
                Icon(Icons.warning, color: Colors.white),
                SizedBox(width: 8),
                Expanded(
                  child: Text('This item has no active stocktake'),
                ),
              ],
            ),
            backgroundColor: Colors.orange,
            duration: const Duration(seconds: 3),
          ),
        );
      }

      if (!context.mounted) return;

      // Przejdź do szczegółów itemu
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => ItemDetailPage(
            item: item,
            api: _api,
          ),
        ),
      );
    } on Exception catch (e) {
      if (!context.mounted) return;
      Navigator.pop(context);

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Row(
            children: [
              const Icon(Icons.warning, color: Colors.white),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  e.toString().contains('Not your item')
                      ? 'This item is not assigned to your stocktake'
                      : 'Error: Item not found',
                ),
              ),
            ],
          ),
          backgroundColor: Colors.orange,
          duration: const Duration(seconds: 3),
        ),
      );
    } catch (e) {
      if (!context.mounted) return;
      Navigator.pop(context);

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Error: $e'),
          backgroundColor: Colors.red,
          duration: const Duration(seconds: 3),
        ),
      );
    }
  }

  void _showCompletionDialog(BuildContext context, Map<String, dynamic> result) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Row(
          children: [
            Icon(Icons.celebration, color: Colors.green, size: 32),
            SizedBox(width: 12),
            Text('Stocktake Completed!'),
          ],
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Congratulations! All items have been checked.',
              style: TextStyle(fontSize: 16),
            ),
            const SizedBox(height: 16),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.green.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Icon(Icons.check_circle, color: Colors.green),
                  const SizedBox(width: 8),
                  Text(
                    '${result['checkedCount']}/${result['totalItems']} items',
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: Colors.green,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              // Odśwież listę po zamknięciu dialogu
              _invalidateCache();
            },
            child: const Text('OK'),
          ),
        ],
      ),
    );
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
              const SizedBox(width: 15),
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
                  await _api.logout();
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