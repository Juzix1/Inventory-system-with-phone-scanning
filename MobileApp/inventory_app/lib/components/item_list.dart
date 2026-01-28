import 'package:flutter/material.dart';
import 'package:inventory_app/Models/inventory_item.dart';
import 'package:inventory_app/pages/item_detail.page.dart';
import 'package:inventory_app/services/apiservice.dart';

class ItemList extends StatefulWidget {
  const ItemList({super.key});

  @override
  State<ItemList> createState() => _ItemListState();
}

class _ItemListState extends State<ItemList> {
  final ApiService _api = ApiService();
  List<InventoryItem>? _items;
  Set<int> _checkedItemIds = {}; // Przechowuj ID sprawdzonych itemów
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadMyItems();
  }

  Future<void> _loadMyItems() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final items = await _api.getMyItems();

      // Pobierz statusy dla wszystkich stocktake
      final checkedIds = <int>{};
      
      // Grupuj itemy po stocktakeId
      final stocktakeIds = items
          .where((item) => item.stocktakeId != null)
          .map((item) => item.stocktakeId!)
          .toSet();

      // Dla każdego stocktake pobierz listę sprawdzonych itemów
      for (final stocktakeId in stocktakeIds) {
        try {
          final progress = await _api.getStocktakeProgress(stocktakeId);
          final ids = List<int>.from(progress['checkedItemIds'] ?? []);
          checkedIds.addAll(ids);
        } catch (e) {
          print('Error loading progress for stocktake $stocktakeId: $e');
        }
      }

      if (mounted) {
        setState(() {
          _items = items;
          _checkedItemIds = checkedIds;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() {
          _error = e.toString();
          _isLoading = false;
        });
      }
    }
  }

  Widget _buildItemImage(InventoryItem item) {
    final imageUrl = _api.getImageUrl(item.imagePath);
    final isChecked = _checkedItemIds.contains(item.id);
    
    if (imageUrl == null || imageUrl.isEmpty) {
      return Stack(
        children: [
          CircleAvatar(
            backgroundColor: isChecked 
                ? Colors.green.shade100 
                : Theme.of(context).colorScheme.primaryContainer,
            radius: 28,
            child: Text(
              item.itemName[0].toUpperCase(),
              style: TextStyle(
                color: isChecked 
                    ? Colors.green.shade800 
                    : Theme.of(context).colorScheme.onPrimaryContainer,
                fontWeight: FontWeight.bold,
                fontSize: 20,
              ),
            ),
          ),
          if (isChecked)
            Positioned(
              right: 0,
              bottom: 0,
              child: Container(
                padding: const EdgeInsets.all(2),
                decoration: BoxDecoration(
                  color: Colors.green,
                  shape: BoxShape.circle,
                  border: Border.all(color: Colors.white, width: 2),
                ),
                child: const Icon(
                  Icons.check,
                  size: 12,
                  color: Colors.white,
                ),
              ),
            ),
        ],
      );
    }

    return Stack(
      children: [
        ClipRRect(
          borderRadius: BorderRadius.circular(8),
          child: Container(
            decoration: BoxDecoration(
              border: isChecked 
                  ? Border.all(color: Colors.green, width: 3)
                  : null,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Image.network(
              imageUrl,
              width: 56,
              height: 56,
              fit: BoxFit.cover,
              errorBuilder: (context, error, stackTrace) {
                return CircleAvatar(
                  backgroundColor: isChecked 
                      ? Colors.green.shade100 
                      : Theme.of(context).colorScheme.primaryContainer,
                  radius: 28,
                  child: Text(
                    item.itemName[0].toUpperCase(),
                    style: TextStyle(
                      color: isChecked 
                          ? Colors.green.shade800 
                          : Theme.of(context).colorScheme.onPrimaryContainer,
                      fontWeight: FontWeight.bold,
                      fontSize: 20,
                    ),
                  ),
                );
              },
              loadingBuilder: (context, child, loadingProgress) {
                if (loadingProgress == null) return child;
                return Container(
                  width: 56,
                  height: 56,
                  decoration: BoxDecoration(
                    color: Colors.grey[200],
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Center(
                    child: CircularProgressIndicator(
                      value: loadingProgress.expectedTotalBytes != null
                          ? loadingProgress.cumulativeBytesLoaded /
                              loadingProgress.expectedTotalBytes!
                          : null,
                      strokeWidth: 2,
                    ),
                  ),
                );
              },
            ),
          ),
        ),
        if (isChecked)
          Positioned(
            right: 0,
            bottom: 0,
            child: Container(
              padding: const EdgeInsets.all(2),
              decoration: BoxDecoration(
                color: Colors.green,
                shape: BoxShape.circle,
                border: Border.all(color: Colors.white, width: 2),
              ),
              child: const Icon(
                Icons.check,
                size: 12,
                color: Colors.white,
              ),
            ),
          ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(
        child: Padding(
          padding: EdgeInsets.all(32.0),
          child: CircularProgressIndicator(),
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(Icons.error_outline, size: 48, color: Colors.red[300]),
              const SizedBox(height: 16),
              const Text(
                'Error loading items',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                _error!,
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey[600]),
              ),
              const SizedBox(height: 16),
              ElevatedButton.icon(
                onPressed: _loadMyItems,
                icon: const Icon(Icons.refresh),
                label: const Text('Retry'),
              ),
            ],
          ),
        ),
      );
    }

    if (_items == null || _items!.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(32.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                Icons.inventory_2_outlined,
                size: 64,
                color: Colors.grey[300],
              ),
              const SizedBox(height: 16),
              Text(
                'No items assigned to you',
                style: TextStyle(fontSize: 18, color: Colors.grey[600]),
              ),
            ],
          ),
        ),
      );
    }

    // Policz sprawdzone i niesprawdzone
    final checkedCount = _items!.where((item) => _checkedItemIds.contains(item.id)).length;
    final totalCount = _items!.length;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Nagłówek z progress
        Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    'Assigned Items: $totalCount',
                    style: const TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.refresh),
                    onPressed: _loadMyItems,
                    tooltip: 'Refresh',
                  ),
                ],
              ),
              const SizedBox(height: 8),
              // Progress bar
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: LinearProgressIndicator(
                          value: totalCount > 0 ? checkedCount / totalCount : 0,
                          backgroundColor: Colors.grey[300],
                          valueColor: AlwaysStoppedAnimation<Color>(Colors.green),
                          minHeight: 8,
                        ),
                      ),
                      const SizedBox(width: 12),
                      Text(
                        '$checkedCount/$totalCount',
                        style: TextStyle(
                          fontWeight: FontWeight.bold,
                          color: checkedCount == totalCount ? Colors.green : null,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '${((checkedCount / totalCount) * 100).toStringAsFixed(0)}% complete',
                    style: TextStyle(
                      fontSize: 12,
                      color: Colors.grey[600],
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),

        // Lista
        Expanded(
          child: RefreshIndicator(
            onRefresh: _loadMyItems,
            child: ListView.builder(
              itemCount: _items!.length,
              padding: const EdgeInsets.symmetric(horizontal: 8),
              itemBuilder: (context, index) {
                final item = _items![index];
                final isChecked = _checkedItemIds.contains(item.id);
                
                return Card(
                  margin: const EdgeInsets.symmetric(
                    vertical: 4,
                    horizontal: 8,
                  ),
                  color: isChecked 
                      ? Colors.green.shade50 
                      : Theme.of(context).colorScheme.primaryContainer,
                  elevation: isChecked ? 1 : 2,
                  child: ListTile(
                    leading: _buildItemImage(item),
                    title: Row(
                      children: [
                        Expanded(
                          child: Text(
                            item.itemName,
                            style: TextStyle(
                              color: isChecked 
                                  ? Colors.green.shade900 
                                  : Theme.of(context).colorScheme.onPrimaryContainer,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                        if (isChecked)
                          Icon(
                            Icons.check_circle,
                            color: Colors.green,
                            size: 20,
                          ),
                      ],
                    ),
                    subtitle: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const SizedBox(height: 4),
                        if (item.itemDescription != null &&
                            item.itemDescription!.isNotEmpty)
                          Text(
                            item.itemDescription!,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: TextStyle(
                              fontSize: 12,
                              color: isChecked 
                                  ? Colors.green.shade700 
                                  : Theme.of(context).colorScheme.tertiary,
                            ),
                          ),
                        Text(
                          item.room ?? 'No room assigned',
                          style: TextStyle(
                            fontSize: 12,
                            color: isChecked 
                                ? Colors.green.shade700 
                                : Theme.of(context).colorScheme.onPrimaryContainer,
                          ),
                        ),
                      ],
                    ),
                    trailing: Container(
                      width: 30,
                      height: 30,
                      decoration: BoxDecoration(
                        color: isChecked 
                            ? Colors.green 
                            : Theme.of(context).colorScheme.onSecondary,
                        borderRadius: BorderRadius.circular(4.0),
                      ),
                      child: Icon(
                        Icons.chevron_right,
                        color: Colors.white,
                      ),
                    ),
                    onTap: () async {
                      await Navigator.push(
                        context,
                        MaterialPageRoute(
                          builder: (context) => ItemDetailPage(
                            item: item,
                            api: _api,
                          ),
                        ),
                      );
                      // Odśwież listę po powrocie ze szczegółów
                      _loadMyItems();
                    },
                  ),
                );
              },
            ),
          ),
        ),
      ],
    );
  }
}