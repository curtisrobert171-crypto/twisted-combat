import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../common/scaffold_page.dart';

class GenerateScreen extends StatelessWidget {
  const GenerateScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return ScaffoldPage(
      title: 'Generate (Stub)',
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Image and text jobs will run through Cloud Functions in Step 4.'),
          const SizedBox(height: 8),
          const Text('Current screen demonstrates the navigation skeleton in local mock mode.'),
          const Spacer(),
          FilledButton(
            onPressed: () => context.go('/export'),
            child: const Text('View Export Pack'),
          ),
        ],
      ),
    );
  }
}
