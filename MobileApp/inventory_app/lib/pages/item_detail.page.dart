import 'package:flutter/material.dart';
import 'package:inventory_app/Models/inventory_item.dart';
import 'package:inventory_app/Models/item_condition.dart';
import 'package:inventory_app/Models/room.dart';
import 'package:inventory_app/services/apiservice.dart';

class ItemDetailPage extends StatefulWidget {
  final InventoryItem item;
  final ApiService api;

  const ItemDetailPage({super.key, required this.item, required this.api});

  @override
  State<ItemDetailPage> createState() => _ItemDetailPageState();
}

class _ItemDetailPageState extends State<ItemDetailPage> {
  late InventoryItem currentItem;
  bool isLoading = false;

  @override
  void initState() {
    super.initState();
    currentItem = widget.item;
  }

  Widget _buildItemImage(BuildContext context) {
    final imageUrl = widget.api.getImageUrl(currentItem.imagePath);

    if (imageUrl == null || imageUrl.isEmpty) {
      return Container(
        height: 250,
        decoration: BoxDecoration(
          color: Theme.of(context).colorScheme.primaryContainer,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(12),
            topRight: Radius.circular(12),
          ),
        ),
        child: Center(
          child: Icon(
            Icons.inventory_2,
            size: 100,
            color: Theme.of(context).colorScheme.onPrimaryContainer,
          ),
        ),
      );
    }

    return ClipRRect(
      borderRadius: const BorderRadius.only(
        topLeft: Radius.circular(12),
        topRight: Radius.circular(12),
      ),
      child: Image.network(
        imageUrl,
        height: 250,
        width: double.infinity,
        fit: BoxFit.cover,
        errorBuilder: (context, error, stackTrace) {
          return Container(
            height: 250,
            decoration: BoxDecoration(
              color: Theme.of(context).colorScheme.primaryContainer,
              borderRadius: BorderRadius.only(
                topLeft: Radius.circular(12),
                topRight: Radius.circular(12),
              ),
            ),
            child: Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Icon(
                    Icons.broken_image,
                    size: 80,
                    color: Theme.of(context).colorScheme.onPrimaryContainer,
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Image not available',
                    style: TextStyle(
                      color: Theme.of(context).colorScheme.onPrimaryContainer,
                    ),
                  ),
                ],
              ),
            ),
          );
        },
        loadingBuilder: (context, child, loadingProgress) {
          if (loadingProgress == null) return child;
          return Container(
            height: 250,
            decoration: BoxDecoration(
              color: Colors.grey[200],
              borderRadius: BorderRadius.circular(12),
            ),
            child: Center(
              child: CircularProgressIndicator(
                value: loadingProgress.expectedTotalBytes != null
                    ? loadingProgress.cumulativeBytesLoaded /
                          loadingProgress.expectedTotalBytes!
                    : null,
              ),
            ),
          );
        },
      ),
    );
  }

  Future<void> _showEditConditionDialog() async {
    final conditions = await widget.api.getAllConditions();
    if (conditions == null || conditions.isEmpty) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('No conditions available')),
        );
      }
      return;
    }

    if (!mounted) return;

    final selectedCondition = await showDialog<ItemCondition>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Select Condition'),
        content: SizedBox(
          width: double.maxFinite,
          child: ListView.builder(
            shrinkWrap: true,
            itemCount: conditions.length,
            itemBuilder: (context, index) {
              final condition = conditions[index];
              final isSelected = currentItem.itemConditionId == condition.id;

              return ListTile(
                title: Text(
                  condition.conditionName,
                  style: TextStyle(
                    fontWeight: isSelected
                        ? FontWeight.bold
                        : FontWeight.normal,
                  ),
                ),
                trailing: isSelected
                    ? Icon(Icons.check_circle, color: Colors.green)
                    : null,
                selected: isSelected,
                selectedTileColor: Theme.of(
                  context,
                ).colorScheme.primaryContainer.withOpacity(0.3),
                onTap: () => Navigator.pop(context, condition),
              );
            },
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
        ],
      ),
    );

    if (selectedCondition != null) {
      await _updateItemCondition(selectedCondition.id);
    }
  }

  Future<void> _showEditLocationDialog() async {
    final rooms = await widget.api.getAllRooms();
    if (rooms == null || rooms.isEmpty) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('No rooms available')));
      }
      return;
    }

    if (!mounted) return;

    final selectedRoom = await showDialog<Room>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Select Room'),
        content: SizedBox(
          width: double.maxFinite,
          child: ListView.builder(
            shrinkWrap: true,
            itemCount: rooms.length,
            itemBuilder: (context, index) {
              final room = rooms[index];
              final isSelected = currentItem.room == room.roomName;

              return ListTile(
                title: Text(
                  room.roomName,
                  style: TextStyle(
                    fontWeight: isSelected
                        ? FontWeight.bold
                        : FontWeight.normal,
                  ),
                ),
                subtitle: room.departmentName != null
                    ? Text(
                        '${room.departmentName}${room.departmentLocation != null ? " (${room.departmentLocation})" : ""}',
                        style: TextStyle(fontSize: 12),
                      )
                    : null,
                trailing: isSelected
                    ? Icon(Icons.check_circle, color: Colors.green)
                    : null,
                selected: isSelected,
                selectedTileColor: Theme.of(
                  context,
                ).colorScheme.primaryContainer.withOpacity(0.3),
                onTap: () => Navigator.pop(context, room),
              );
            },
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Cancel'),
          ),
        ],
      ),
    );

    if (selectedRoom != null) {
      await _updateItemLocation(selectedRoom.id);
    }
  }

  Future<void> _updateItemCondition(int conditionId) async {
    setState(() => isLoading = true);

    try {
      final updatedItem = await widget.api.updateItemCondition(
        currentItem.id,
        conditionId,
      );

      setState(() {
        currentItem = updatedItem;
        isLoading = false;
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Row(
              children: [
                Icon(Icons.check_circle, color: Colors.white),
                SizedBox(width: 8),
                Text('Condition updated successfully'),
              ],
            ),
            backgroundColor: Colors.green,
            duration: Duration(seconds: 2),
          ),
        );
      }
    } catch (e) {
      setState(() => isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Row(
              children: [
                Icon(Icons.error, color: Colors.white),
                SizedBox(width: 8),
                Expanded(child: Text('Error: $e')),
              ],
            ),
            backgroundColor: Colors.red,
            duration: Duration(seconds: 4),
          ),
        );
      }
    }
  }

  Future<void> _updateItemLocation(int roomId) async {
    setState(() => isLoading = true);

    try {
      final updatedItem = await widget.api.updateItemLocation(
        currentItem.id,
        roomId,
      );

      setState(() {
        currentItem = updatedItem;
        isLoading = false;
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Row(
              children: [
                Icon(Icons.check_circle, color: Colors.white),
                SizedBox(width: 8),
                Text('Location updated successfully'),
              ],
            ),
            backgroundColor: Colors.green,
            duration: Duration(seconds: 2),
          ),
        );
      }
    } catch (e) {
      setState(() => isLoading = false);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Row(
              children: [
                Icon(Icons.error, color: Colors.white),
                SizedBox(width: 8),
                Expanded(child: Text('Error: $e')),
              ],
            ),
            backgroundColor: Colors.red,
            duration: Duration(seconds: 4),
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: Theme.of(context).colorScheme.secondaryContainer,
        title: Text(
          "Item Details",
          style: TextStyle(
            fontWeight: FontWeight.bold,
            color: Theme.of(context).colorScheme.surface,
          ),
        ),
      ),
      body: Stack(
        children: [
          SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Image Card
                _buildItemImage(context),
                // Header Card
                Container(
                  decoration: BoxDecoration(
                    color: Theme.of(context).colorScheme.primaryContainer,
                    borderRadius: BorderRadius.only(
                      bottomLeft: Radius.circular(12),
                      bottomRight: Radius.circular(12),
                    ),
                  ),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Row(
                      children:[ Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            currentItem.itemName,
                            style: TextStyle(
                              fontSize: 24,
                              fontWeight: FontWeight.bold,
                              color: Theme.of(context).colorScheme.surface,
                            ),
                          ),
                          if (currentItem.itemDescription != null)
                            Padding(
                              padding: const EdgeInsets.only(top: 8),
                              child: Text(
                                currentItem.itemDescription!,
                                style: TextStyle(
                                  fontSize: 16,
                                  color: Theme.of(
                                    context,
                                  ).colorScheme.onPrimaryContainer,
                                ),
                              ),
                            ),
                        ],
                      ),
                      ]
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Details Card
                Card(
                  color: Theme.of(context).colorScheme.primaryContainer,
                  elevation: 2,
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Details',
                              style: TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                color: Theme.of(
                                  context,
                                ).colorScheme.onPrimaryContainer,
                              ),
                            ),
                            IconButton(
                              icon: const Icon(Icons.edit),
                              onPressed: isLoading
                                  ? null
                                  : _showEditConditionDialog,
                              tooltip: 'Edit Condition',
                              color: Theme.of(context).colorScheme.primary,
                            ),
                          ],
                        ),
                        Divider(
                          color: Theme.of(
                            context,
                          ).colorScheme.onPrimaryContainer,
                        ),
                        _buildDetailRow(
                          'ID',
                          currentItem.id.toString(),
                          context,
                        ),
                        _buildDetailRow(
                          'Price',
                          currentItem.formattedPrice,
                          context,
                        ),
                        _buildDetailRow(
                          'Weight',
                          currentItem.formattedWeight,
                          context,
                        ),
                        _buildDetailRow(
                          'Type',
                          currentItem.itemType.toString(),
                          context,
                        ),
                        if (currentItem.itemConditionId != null)
                          _buildDetailRow(
                            'Condition',
                            currentItem.itemConditionId.toString(),
                            context,
                          ),
                        _buildDetailRow(
                          'Added',
                          currentItem.formattedAddedDate,
                          context,
                        ),
                        _buildDetailRow(
                          'Last Inventory',
                          currentItem.formattedLastInventoryDate,
                          context,
                        ),
                        if (currentItem.warrantyEnd != null)
                          _buildDetailRow(
                            'Warranty',
                            currentItem.formattedWarrantyEnd,
                            context,
                            trailing: currentItem.hasWarranty
                                ? const Icon(
                                    Icons.check_circle,
                                    color: Colors.green,
                                  )
                                : const Icon(Icons.cancel, color: Colors.red),
                          ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Location Card
                Card(
                  color: Theme.of(context).colorScheme.onSecondary,
                  elevation: 2,
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Location',
                              style: TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                color: Theme.of(context).colorScheme.surface,
                              ),
                            ),
                            IconButton(
                              icon: const Icon(Icons.edit),
                              onPressed: isLoading
                                  ? null
                                  : _showEditLocationDialog,
                              tooltip: 'Edit Location',
                              color: Theme.of(context).colorScheme.primary,
                            ),
                          ],
                        ),
                        Divider(color: Theme.of(context).colorScheme.secondary),
                        _buildDetailRow(
                          'Room',
                          currentItem.room ?? 'Not assigned',
                          context,
                        ),
                        if (currentItem.personInChargeId != null)
                          _buildDetailRow(
                            'Person in Charge ID',
                            currentItem.personInChargeId.toString(),
                            context,
                          ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
          if (isLoading)
            Container(
              color: Colors.black.withOpacity(0.5),
              child: Center(
                child: Card(
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        CircularProgressIndicator(),
                        SizedBox(height: 16),
                        Text(
                          'Updating...',
                          style: TextStyle(
                            fontSize: 16,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildDetailRow(
    String label,
    String value,
    BuildContext context, {
    Widget? trailing,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Text(
            label,
            style: TextStyle(
              fontWeight: FontWeight.w500,
              fontSize: 16,
              color: Theme.of(context).colorScheme.surface,
            ),
          ),
          Expanded(
            child: Row(
              mainAxisAlignment: MainAxisAlignment.end,
              mainAxisSize: MainAxisSize.min,
              children: [
                Flexible(
                  child: Text(
                    value,
                    style: TextStyle(
                      fontSize: 16,
                      color: Theme.of(context).colorScheme.onPrimaryContainer,
                    ),
                    textAlign: TextAlign.right,
                    overflow: TextOverflow.ellipsis,
                  ),
                ),
                if (trailing != null) ...[const SizedBox(width: 8), trailing],
              ],
            ),
          ),
        ],
      ),
    );
  }
}
