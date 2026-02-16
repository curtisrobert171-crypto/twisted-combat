import 'package:flutter_riverpod/flutter_riverpod.dart';

enum ListingCategory { furniture, electronics, appliances, tools, babyKids }
enum EditStyle { cleanStudio, roomBlur, lightingOnly }

class PackDraft {
  const PackDraft({
    this.category,
    this.photoPaths = const [],
    this.style = EditStyle.cleanStudio,
    this.formValues = const {},
  });

  final ListingCategory? category;
  final List<String> photoPaths;
  final EditStyle style;
  final Map<String, dynamic> formValues;

  PackDraft copyWith({
    ListingCategory? category,
    List<String>? photoPaths,
    EditStyle? style,
    Map<String, dynamic>? formValues,
  }) {
    return PackDraft(
      category: category ?? this.category,
      photoPaths: photoPaths ?? this.photoPaths,
      style: style ?? this.style,
      formValues: formValues ?? this.formValues,
    );
  }
}

class PackDraftController extends StateNotifier<PackDraft> {
  PackDraftController() : super(const PackDraft());

  void selectCategory(ListingCategory category) {
    state = state.copyWith(category: category);
  }

  void setPhotos(List<String> photos) {
    state = state.copyWith(photoPaths: photos);
  }

  void setStyle(EditStyle style) {
    state = state.copyWith(style: style);
  }

  void setFormValues(Map<String, dynamic> formValues) {
    state = state.copyWith(formValues: formValues);
  }
}

final packDraftProvider = StateNotifierProvider<PackDraftController, PackDraft>(
  (ref) => PackDraftController(),
);
