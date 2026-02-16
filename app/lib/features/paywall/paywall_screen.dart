import 'package:flutter/material.dart';

import '../common/scaffold_page.dart';

class PaywallScreen extends StatelessWidget {
  const PaywallScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return ScaffoldPage(
      title: 'Go Pro',
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Stub paywall: monthly / annual variants from Remote Config in Step 7.'),
          const SizedBox(height: 12),
          Card(
            child: ListTile(
              title: const Text('Pro Monthly - $9.99'),
              subtitle: const Text('More credits + unlimited pack exports'),
              trailing: FilledButton(onPressed: () {}, child: const Text('Subscribe')),
            ),
          ),
        ],
      ),
    );
  }
}
