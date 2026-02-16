import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../common/scaffold_page.dart';

class OnboardingScreen extends StatelessWidget {
  const OnboardingScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return ScaffoldPage(
      title: 'AI Marketplace Seller Studio',
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Generate better listing photos and text packs for Facebook Marketplace in minutes.',
          ),
          const SizedBox(height: 16),
          const Text('MVP Flow: Category → Photos → Edit Style → Form → Generate → Export.'),
          const Spacer(),
          FilledButton(
            onPressed: () => context.go('/category'),
            child: const Text('Get Started'),
          ),
        ],
      ),
    );
  }
}
