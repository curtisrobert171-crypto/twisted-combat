import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../features/export/checklist_screen.dart';
import '../features/export/export_screen.dart';
import '../features/form/form_screen.dart';
import '../features/generate/generate_screen.dart';
import '../features/onboarding/onboarding_screen.dart';
import '../features/paywall/paywall_screen.dart';
import '../features/photos/photos_screen.dart';
import '../features/style/style_screen.dart';
import '../features/templates/category_screen.dart';

final appRouterProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(path: '/', builder: (_, __) => const OnboardingScreen()),
      GoRoute(path: '/category', builder: (_, __) => const CategoryScreen()),
      GoRoute(path: '/photos', builder: (_, __) => const PhotosScreen()),
      GoRoute(path: '/style', builder: (_, __) => const StyleScreen()),
      GoRoute(path: '/form', builder: (_, __) => const FormScreen()),
      GoRoute(path: '/generate', builder: (_, __) => const GenerateScreen()),
      GoRoute(path: '/export', builder: (_, __) => const ExportScreen()),
      GoRoute(path: '/checklist', builder: (_, __) => const ChecklistScreen()),
      GoRoute(path: '/paywall', builder: (_, __) => const PaywallScreen()),
    ],
  );
});
