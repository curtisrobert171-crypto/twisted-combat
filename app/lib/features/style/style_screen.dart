import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../common/draft.dart';
import '../common/scaffold_page.dart';

class StyleScreen extends ConsumerWidget {
  const StyleScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final draft = ref.watch(packDraftProvider);
    final controller = ref.read(packDraftProvider.notifier);

    return ScaffoldPage(
      title: 'Choose Edit Style',
      body: Column(
        children: [
          RadioListTile<EditStyle>(
            value: EditStyle.cleanStudio,
            groupValue: draft.style,
            onChanged: (value) => controller.setStyle(value!),
            title: const Text('Clean Studio'),
          ),
          RadioListTile<EditStyle>(
            value: EditStyle.roomBlur,
            groupValue: draft.style,
            onChanged: (value) => controller.setStyle(value!),
            title: const Text('Room Blur'),
          ),
          RadioListTile<EditStyle>(
            value: EditStyle.lightingOnly,
            groupValue: draft.style,
            onChanged: (value) => controller.setStyle(value!),
            title: const Text('Lighting Only'),
          ),
          const Spacer(),
          FilledButton(
            onPressed: () => context.go('/form'),
            child: const Text('Continue'),
          ),
        ],
      ),
    );
  }
}
