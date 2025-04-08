import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';

import 'package:dart_codelab/main.dart'; // Update with your actual package name

void main() {
  testWidgets('Test selectedIndex switching', (WidgetTester tester) async {
    await tester.pumpWidget(
      ChangeNotifierProvider(
        create: (_) => MyAppState(),
        child: MaterialApp(
          home: MyHomePage(),
        ),
      ),
    );

    // Initially, GeneratorPage should be shown (check for a word pair on screen)
    expect(find.byType(GeneratorPage), findsOneWidget);
    expect(find.byType(FavoritesPage), findsNothing);

    // Tap the Favorites icon in the BottomNavigationBar
    await tester.tap(find.byIcon(Icons.favorite));
    await tester.pumpAndSettle();

    // Now, the FavoritesPage should be shown
    expect(find.byType(GeneratorPage), findsNothing);
    expect(find.byType(FavoritesPage), findsOneWidget);

    // Tap the Home icon again
    await tester.tap(find.byIcon(Icons.home));
    await tester.pumpAndSettle();

    // Should return to GeneratorPage
    expect(find.byType(GeneratorPage), findsOneWidget);
    expect(find.byType(FavoritesPage), findsNothing);
  });
}
