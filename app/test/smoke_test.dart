import 'package:ai_marketplace_seller_studio/app/app.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  testWidgets('renders onboarding', (tester) async {
    await tester.pumpWidget(const ProviderScope(child: SellerStudioApp()));
    expect(find.text('AI Marketplace Seller Studio'), findsOneWidget);
  });
}
