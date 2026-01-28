import 'package:flutter/material.dart';
import 'package:inventory_app/Models/inventory_item.dart';

class ItemDetailPage extends StatelessWidget {
  final InventoryItem item;

  const ItemDetailPage({super.key, required this.item});

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
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header Card
            Card(
              color: Theme.of(context).colorScheme.primaryContainer,
              child: Expanded(
                child: Row(
                  children: [
                    Padding(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            item.itemName,
                            style: TextStyle(
                              fontSize: 24,
                              fontWeight: FontWeight.bold,
                              color: Theme.of(context).colorScheme.surface,
                            ),
                          ),
                          if (item.itemDescription != null)
                            Padding(
                              padding: const EdgeInsets.only(top: 8),
                              child: Text(
                                item.itemDescription!,
                                style: TextStyle(
                                  fontSize: 16,
                                  color: Theme.of(context).colorScheme.onPrimaryContainer,
                                ),
                              ),
                            ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),

            Card(
              color: Theme.of(context).colorScheme.primaryContainer,
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                     Text(
                      'Details',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: Theme.of(context).colorScheme.onPrimaryContainer,
                      ),
                    ),
                     Divider(color: Theme.of(context).colorScheme.onPrimaryContainer,),
                    _buildDetailRow('ID', item.id.toString(), context),
                    _buildDetailRow('Price', item.formattedPrice, context),
                    _buildDetailRow('Weight', item.formattedWeight, context),
                    _buildDetailRow('Type', item.itemType.toString(), context),
                    if (item.itemConditionId != null)
                      _buildDetailRow(
                        'Condition',
                        item.itemConditionId.toString(),
                        context
                      ),
                    _buildDetailRow('Added', item.formattedAddedDate,context),
                    _buildDetailRow('Last Inventory', item.formattedAddedDate,context),
                    if (item.warrantyEnd != null)
                      _buildDetailRow(
                        'Warranty',
                        item.formattedWarrantyEnd,
                        context,
                        trailing: item.hasWarranty
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
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Location',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: Theme.of(context).colorScheme.surface
                      ),
                    ),
                    Divider(color: Theme.of(context).colorScheme.secondary,),
                    _buildDetailRow('Room', item.room.toString(),context),
                    if (item.personInChargeId != null)
                      _buildDetailRow(
                        'Person in Charge ID',
                        item.personInChargeId.toString(),
                        context
                      ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDetailRow(String label, String value, BuildContext context,{Widget? trailing}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(fontWeight: FontWeight.w500, fontSize: 16, color:Theme.of(context).colorScheme.surface,),
          ),
          Row(
            children: [
              Text(
                value,
                style: TextStyle(fontSize: 16, color: Theme.of(context).colorScheme.onPrimaryContainer),
              ),
              if (trailing != null) ...[const SizedBox(width: 8), trailing],
            ],
          ),
        ],
      ),
    );
  }
}
