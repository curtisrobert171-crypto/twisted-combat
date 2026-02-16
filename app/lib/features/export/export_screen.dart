import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../common/scaffold_page.dart';

class ExportScreen extends StatelessWidget {
  const ExportScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return ScaffoldPage(
      title: 'Export Pack',
      body: ListView(
        children: [
          const ListTile(title: Text('Images: square / portrait / landscape (stub)')),
          const ListTile(title: Text('Copy title, description, tags, quick replies (stub)')),
          const SizedBox(height: 16),
          FilledButton(
            onPressed: () => context.go('/checklist'),
            child: const Text('How to post on Facebook'),
          ),
          const SizedBox(height: 8),
          OutlinedButton(
            onPressed: () => context.go('/paywall'),
            child: const Text('View Paywall'),
          ),
        ],
      ),
    );
  }
}
