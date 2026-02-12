import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../common/draft.dart';
import '../common/scaffold_page.dart';

class CategoryScreen extends ConsumerWidget {
  const CategoryScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final categories = ListingCategory.values;

    return ScaffoldPage(
      title: 'Pick a Category',
      body: ListView.separated(
        itemCount: categories.length,
        separatorBuilder: (_, __) => const SizedBox(height: 8),
        itemBuilder: (context, index) {
          final category = categories[index];
          return ListTile(
            tileColor: Colors.grey.shade100,
            title: Text(_label(category)),
            trailing: const Icon(Icons.chevron_right),
            onTap: () {
              ref.read(packDraftProvider.notifier).selectCategory(category);
              context.go('/photos');
            },
          );
        },
      ),
    );
  }

  String _label(ListingCategory category) {
    switch (category) {
      case ListingCategory.furniture:
        return 'Furniture';
      case ListingCategory.electronics:
        return 'Electronics';
      case ListingCategory.appliances:
        return 'Appliances';
      case ListingCategory.tools:
        return 'Tools';
      case ListingCategory.babyKids:
        return 'Baby & Kids';
    }
  }
}
