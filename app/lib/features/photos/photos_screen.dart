import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../common/draft.dart';
import '../common/scaffold_page.dart';

class PhotosScreen extends ConsumerWidget {
  const PhotosScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final draft = ref.watch(packDraftProvider);
    final controller = ref.read(packDraftProvider.notifier);

    return ScaffoldPage(
      title: 'Add 3–8 Photos',
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Selected: ${draft.photoPaths.length} photos'),
          const SizedBox(height: 12),
          Wrap(
            spacing: 8,
            children: List.generate(
              draft.photoPaths.length,
              (i) => Chip(label: Text('Photo ${i + 1}')),
            ),
          ),
          const SizedBox(height: 12),
          OutlinedButton.icon(
            onPressed: () {
              final next = List<String>.from(draft.photoPaths)
                ..add('mock_photo_${draft.photoPaths.length + 1}.jpg');
              if (next.length <= 8) controller.setPhotos(next);
            },
            icon: const Icon(Icons.add_a_photo_outlined),
            label: const Text('Add mock photo'),
          ),
          const Spacer(),
          FilledButton(
            onPressed: draft.photoPaths.length >= 3
                ? () => context.go('/style')
                : null,
            child: const Text('Continue'),
          ),
        ],
      ),
    );
  }
}
