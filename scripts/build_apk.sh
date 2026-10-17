#!/bin/bash
set -e

echo "[APK BUILD] Starting Android APK build..."

# Clean old intermediates
rm -rf android_build/gen/* android_build/bin/* android_build/assets/*
mkdir -p android_build/gen android_build/bin/classes android_build/bin/apk_root android_build/assets app/build/outputs/apk/debug

# 1. Sync web dist into Android assets
if [ -d "dist" ]; then
    cp -r dist/* android_build/assets/
fi

PLATFORM_JAR="/usr/lib/android-sdk/platforms/android-23/android.jar"

# 2. Generate R.java
echo "[APK BUILD] Generating R.java..."
aapt package -f -m \
    -J android_build/gen \
    -M android_build/AndroidManifest.xml \
    -S android_build/res \
    -I "$PLATFORM_JAR"

# 3. Compile Java sources
echo "[APK BUILD] Compiling Java classes..."
javac -source 1.8 -target 1.8 \
    -d android_build/bin/classes \
    -classpath "$PLATFORM_JAR" \
    android_build/gen/com/aistudio/simitone/R.java \
    android_build/src/com/aistudio/simitone/MainActivity.java

# 4. Convert bytecode to Dalvik dex
echo "[APK BUILD] Translating bytecode with dx..."
dx --dex --output=android_build/bin/apk_root/classes.dex android_build/bin/classes

# 5. Package APK with assets and resources
echo "[APK BUILD] Packaging APK..."
aapt package -f \
    -M android_build/AndroidManifest.xml \
    -S android_build/res \
    -A android_build/assets \
    -I "$PLATFORM_JAR" \
    -F android_build/bin/app-unsigned.apk \
    android_build/bin/apk_root

# 6. Zipalign APK
echo "[APK BUILD] Aligning APK with zipalign..."
zipalign -f -p 4 android_build/bin/app-unsigned.apk android_build/bin/app-aligned.apk

# 7. Create debug keystore if missing
if [ ! -f "debug.keystore" ]; then
    echo "[APK BUILD] Generating debug.keystore..."
    keytool -genkey -v \
        -keystore debug.keystore \
        -storepass android \
        -alias androiddebugkey \
        -keypass android \
        -keyalg RSA \
        -keysize 2048 \
        -validity 10000 \
        -dname "CN=Android Debug,O=Android,C=US"
fi

# 8. Sign APK
echo "[APK BUILD] Signing APK with jarsigner..."
jarsigner -sigalg SHA1withRSA -digestalg SHA1 \
    -keystore debug.keystore \
    -storepass android \
    -keypass android \
    android_build/bin/app-aligned.apk androiddebugkey

# 9. Copy to official AI Studio APK destination
cp -f android_build/bin/app-aligned.apk app/build/outputs/apk/debug/app-debug.apk

echo "[APK BUILD] APK successfully created at app/build/outputs/apk/debug/app-debug.apk"
ls -lh app/build/outputs/apk/debug/app-debug.apk
