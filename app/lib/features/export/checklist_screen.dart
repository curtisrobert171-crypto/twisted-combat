import 'package:flutter/material.dart';

import '../common/scaffold_page.dart';

class ChecklistScreen extends StatelessWidget {
  const ChecklistScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return const ScaffoldPage(
      title: 'Facebook Posting Checklist',
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('1. Open Facebook Marketplace and tap Create Listing.'),
          Text('2. Upload exported photos.'),
          Text('3. Paste title + description + tags.'),
          Text('4. Select category, location, and condition.'),
          Text('5. Publish and monitor inbox using quick replies.'),
        ],
      ),
    );
  }
}
