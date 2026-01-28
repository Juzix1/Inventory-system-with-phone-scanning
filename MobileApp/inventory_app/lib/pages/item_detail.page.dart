import 'package:flutter/material.dart';
import 'package:inventory_app/Models/inventory_item.dart';

class ItemDetailPage extends StatelessWidget {
  final InventoryItem item;

  const ItemDetailPage({super.key, required this.item});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text("Item Details"),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header Card
            Card(
              child: Expanded(
                child: Row(
                  children:[ Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          item.itemName,
                          style: const TextStyle(
                            fontSize: 24,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        if (item.itemDescription != null)
                          Padding(
                            padding: const EdgeInsets.only(top: 8),
                            child: Text(
                              item.itemDescription!,
                              style: TextStyle(
                                fontSize: 16,
                                color: Colors.grey[600],
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

            // Details Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Details',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const Divider(),
                    _buildDetailRow('ID', item.id.toString()),
                    _buildDetailRow('Price', item.formattedPrice),
                    _buildDetailRow('Weight', item.formattedWeight),
                    _buildDetailRow('Type', item.itemType.toString()),
                    if (item.itemConditionId != null)
                      _buildDetailRow('Condition', item.itemConditionId.toString()),
                    _buildDetailRow('Added', item.formattedAddedDate),
                    _buildDetailRow('Last Inventory', item.formattedAddedDate),
                    if (item.warrantyEnd != null)
                      _buildDetailRow(
                        'Warranty',
                        item.formattedWarrantyEnd,
                        trailing: item.hasWarranty
                            ? const Icon(Icons.check_circle, color: Colors.green)
                            : const Icon(Icons.cancel, color: Colors.red),
                      ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),

            // Location Card
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'Location',
                      style: TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const Divider(),
                    _buildDetailRow('Room', item.room.toString()),
                    if (item.personInChargeId != null)
                      _buildDetailRow('Person in Charge ID', item.personInChargeId.toString()),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDetailRow(String label, String value, {Widget? trailing}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: const TextStyle(
              fontWeight: FontWeight.w500,
              fontSize: 16,
            ),
          ),
          Row(
            children: [
              Text(
                value,
                style: TextStyle(
                  fontSize: 16,
                  color: Colors.grey[700],
                ),
              ),
              if (trailing != null) ...[
                const SizedBox(width: 8),
                trailing,
              ],
            ],
          ),
        ],
      ),
    );
  }
}