using System;
using System.IO;
using System.Threading;
using Android.Content;
using FSO.Common;
using FSO.Files;
using FSO.Client;

namespace Simitone.Android.Platform
{
    /// <summary>
    /// Prepares the FreeSO/Simitone engine for Android.
    /// </summary>
    /// <remarks>
    /// This is the Android counterpart of <c>Simitone.Windows.Program</c>. The
    /// engine itself is not modified - everything that is different on Android
    /// (paths, GLES version, DPI scaling, the touch mode) is expressed through
    /// the existing <see cref="FSOEnvironment"/> / <see cref="GlobalSettings"/>
    /// switches.
    /// </remarks>
    public static class SimitoneAndroidBootstrap
    {
        /// <summary>
        /// Applies the launcher settings to the engine. Has to run before the
        /// game object is created.
        /// </summary>
        /// <param name="context">The Android context of the game activity.</param>
        /// <param name="settings">The settings the user configured.</param>
        public static void Configure(Context context, SimitoneAndroidSettings settings)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (settings == null) throw new ArgumentNullException("settings");

            var filesDir = context.FilesDir.AbsolutePath;

            // --- directories ---------------------------------------------------
            var userDir = Path.Combine(filesDir, "Simitone");
            Directory.CreateDirectory(userDir);
            FSOEnvironment.UserDir = userDir.Replace('\\', '/') + "/";

            // The loose content (UI scripts, cities, meshes, CAS database, ...)
            // is unpacked from the APK assets once and then read from disk.
            AndroidContentInstaller.Install(context, filesDir);
            FSOEnvironment.ContentDir = Path.Combine(filesDir, "Content").Replace('\\', '/') + "/";

            // --- graphics ------------------------------------------------------
            var compatibilityMode = settings.Graphics == GraphicsMode.OpenGLEs2Compatibility;
            FSOEnvironment.DirectX = false;
            FSOEnvironment.Linux = true;              // no hardware cursor on Android
            FSOEnvironment.GFXContentDir = compatibilityMode ? "Content/iOS/" : "Content/OGL/";
            FSOEnvironment.GLVer = compatibilityMode ? 2 : 3;
            FSOEnvironment.SoftwareDepth = compatibilityMode;
            FSOEnvironment.UseMRT = !compatibilityMode;
            FSOEnvironment.TexCompress = false;
            FSOEnvironment.EnableNPOTMip = false;
            FSOEnvironment.MSAASupport = !compatibilityMode;
            FSOEnvironment.Enable3D = settings.Enable3D;

            // --- input ---------------------------------------------------------
            // SoftwareKeyboard switches the engine into its touch mode: the
            // keyboard is empty and every touch becomes a virtual mouse.
            FSOEnvironment.SoftwareKeyboard = true;

            // --- display -------------------------------------------------------
            ApplyDisplaySettings(context, settings);

            // --- game data -----------------------------------------------------
            FSOEnvironment.GameThread = Thread.CurrentThread;
            FSOEnvironment.Args = "";
            FSOEnvironment.NoSound = false;

            // Images are decoded with the Android bitmap API.
            FSO.Files.ImageLoaderHelpers.BitmapFunction = AndroidBitmapReader.Read;

            // --- settings ------------------------------------------------------
            if (GlobalSettings.Default.LanguageCode == 0) GlobalSettings.Default.LanguageCode = 1;
            FSO.Files.Formats.IFF.Chunks.STR.DefaultLangCode =
                (FSO.Files.Formats.IFF.Chunks.STRLangCode)GlobalSettings.Default.LanguageCode;

            var gamePath = (settings.GamePath ?? "").Replace('\\', '/');
            if (!gamePath.EndsWith("/")) gamePath += "/";

            GlobalSettings.Default.StartupPath = gamePath;
            GlobalSettings.Default.TS1HybridEnable = true;
            GlobalSettings.Default.TS1HybridPath = gamePath;
            GlobalSettings.Default.ClientVersion = "0";
            GlobalSettings.Default.LightingMode = 3;
            GlobalSettings.Default.AntiAlias = 0;
            GlobalSettings.Default.ComplexShaders = !compatibilityMode;
            GlobalSettings.Default.EnableTransitions = true;
            GlobalSettings.Default.Windowed = true;
        }

        /// <summary>
        /// Computes the DPI scale factor (which defines the logical resolution
        /// of the game) and the UI zoom factor.
        /// </summary>
        private static void ApplyDisplaySettings(Context context, SimitoneAndroidSettings settings)
        {
            var metrics = context.Resources?.DisplayMetrics;
            int deviceWidth = metrics?.WidthPixels ?? 1280;
            int deviceHeight = metrics?.HeightPixels ?? 720;
            int densityDpi = metrics?.DensityDpi ?? 160;

            // The device is locked to landscape, so the "long" side is the width.
            if (deviceHeight > deviceWidth)
            {
                var swap = deviceWidth;
                deviceWidth = deviceHeight;
                deviceHeight = swap;
            }

            settings.ResolveResolution(deviceWidth, deviceHeight, out int targetWidth, out _);

            // The logical width of the game. The height follows the aspect ratio
            // of the device so that nothing is stretched.
            float dpiScale = targetWidth > 0 ? deviceWidth / (float)targetWidth : 1f;
            if (dpiScale <= 0f) dpiScale = 1f;

            FSOEnvironment.DPIScaleFactor = dpiScale;

            // FreeSO scales some panels (pie menu, UCP) with the UI zoom factor.
            // On Android the automatic value takes the screen density into
            // account, the same way the legacy Android build did.
            float baseZoom = 1f + densityDpi / 200f;
            float uiZoom;
            switch (settings.UiScale)
            {
                case UiScaleOption.X100: uiZoom = 1f; break;
                case UiScaleOption.X125: uiZoom = 1.25f; break;
                case UiScaleOption.X150: uiZoom = 1.5f; break;
                case UiScaleOption.X200: uiZoom = 2f; break;
                default: uiZoom = baseZoom / Math.Max(0.25f, dpiScale); break;
            }
            if (uiZoom < 1f) uiZoom = 1f;
            if (uiZoom > 2.5f) uiZoom = 2.5f;
            FSOEnvironment.UIZoomFactor = uiZoom;
        }
    }
}
