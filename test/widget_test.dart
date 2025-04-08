// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:dart_codelab/main.dart';

void main() {
  testWidgets('Counter increments smoke test', (WidgetTester tester) async {
    // Build our app and trigger a frame.
    await tester.pumpWidget(const MyApp());

    // Verify that our left bar created.
    expect(find.text('Home'), findsOneWidget);
    expect(find.text('Favorites'), findsOneWidget);
    expect(
      find.byWidgetPredicate((widget) =>
        widget is Text && widget.data != null && widget.data!.contains('no widget for')
      ),
      findsNothing,
    );

    // Tap the '+' icon and trigger a frame.
    await tester.tap(find.byIcon(Icons.home));
    await tester.pump();

    // Verify that our counter has incremented.
    expect(find.text('Like'), findsOneWidget);
    
  });
}
