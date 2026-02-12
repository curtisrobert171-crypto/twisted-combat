# AI Marketplace Seller Studio (Android MVP)

This repository is scaffolded for an Android-only Flutter + Firebase app that helps sellers create:

1. Improved listing photos.
2. Listing text packs (title options, description, tags, quick replies).

Current delivery status: **Step 1 scaffold complete** (navigation flow, core screens, local mock draft state, project structure).

## Repository Structure

```text
/
  app/                # Flutter app (Android-first)
  functions/          # Firebase Functions (TypeScript scaffold)
  firestore.rules     # Firestore rules scaffold
  storage.rules       # Storage rules scaffold
  firestore_seed/     # Template seed JSON files
  README.md
```

## Prerequisites

- Flutter stable installed and on `PATH`
- Android SDK + emulator/device configured
- Node.js 20+
- Firebase CLI (`npm i -g firebase-tools`)

## Run Flutter App (Scaffold)

```bash
cd app
flutter pub get
flutter run -d android
```

## Run Tests

```bash
cd app
flutter test
```

## Build Debug APK

```bash
cd app
flutter build apk --debug
```

APK output path:

```text
app/build/app/outputs/flutter-apk/app-debug.apk
```

## Functions Scaffold

```bash
cd functions
npm install
npm run build
```

Optional local emulator command (after Firebase project setup in later steps):

```bash
npm run serve
```

## Seed Templates

Template starter data for 5 categories is available at:

- `firestore_seed/templates/templates.json`

## Stub Mode Note

Functions are scaffolded with `AI_STUB_MODE=true` default behavior via env fallback. Full image/text job processing and credit enforcement are implemented in upcoming steps.

## Next Planned Steps

1. Firebase Auth + Firestore/Storage wiring.
2. Dynamic template loader + form renderer.
3. Callable Functions: `createImageJob` / `createTextJob` with stub provider.
4. Credits enforcement + daily caps.
5. Export tooling + clipboard + gallery/share.
6. Paywall abstraction + Remote Config variants.
