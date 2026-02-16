import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../common/draft.dart';
import '../common/scaffold_page.dart';

class FormScreen extends ConsumerStatefulWidget {
  const FormScreen({super.key});

  @override
  ConsumerState<FormScreen> createState() => _FormScreenState();
}

class _FormScreenState extends ConsumerState<FormScreen> {
  final _condition = TextEditingController();
  final _brand = TextEditingController();
  final _price = TextEditingController();

  @override
  Widget build(BuildContext context) {
    final draft = ref.watch(packDraftProvider);
    final category = draft.category;

    return ScaffoldPage(
      title: 'Listing Details',
      body: ListView(
        children: [
          Text('Category: ${category?.name ?? 'unknown'}'),
          const SizedBox(height: 12),
          TextField(controller: _condition, decoration: const InputDecoration(labelText: 'Condition')),
          TextField(controller: _brand, decoration: const InputDecoration(labelText: 'Brand / Maker')),
          TextField(controller: _price, decoration: const InputDecoration(labelText: 'Target Price')),
          const SizedBox(height: 24),
          FilledButton(
            onPressed: () {
              ref.read(packDraftProvider.notifier).setFormValues({
                'condition': _condition.text,
                'brand': _brand.text,
                'targetPrice': _price.text,
              });
              context.go('/generate');
            },
            child: const Text('Generate Pack'),
          ),
        ],
      ),
    );
  }
}
