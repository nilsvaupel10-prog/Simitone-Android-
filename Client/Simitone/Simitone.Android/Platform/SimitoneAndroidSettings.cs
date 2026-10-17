using System;
using System.Collections.Generic;
using System.Globalization;
using Android.Content;

namespace Simitone.Android.Platform
{
    /// <summary>The window sizes the launcher offers.</summary>
    public enum ResolutionPreset
    {
        DeviceNative,
        W1280x720,
        W1600x900,
        W1920x1080,
        W2560x1440,
        Custom
    }

    /// <summary>The graphics backends the launcher offers.</summary>
    public enum GraphicsMode
    {
        /// <summary>OpenGL ES 3.0 - the modern path, uses the "Content/OGL" shaders.</summary>
        OpenGLEs3,
        /// <summary>OpenGL ES 2.0 compatibility mode - uses the "Content/iOS" shaders.</summary>
        OpenGLEs2Compatibility
    }

    /// <summary>The UI scaling options the launcher offers.</summary>
    public enum UiScaleOption
    {
        Automatic,
        X100,
        X125,
        X150,
        X200
    }

    /// <summary>
    /// Everything the launcher screen can configure, persisted in the shared
    /// preferences of the app.
    /// </summary>
    public class SimitoneAndroidSettings
    {
        /// <summary>Folder that holds the user's copy of The Sims 1.</summary>
        public string GamePath { get; set; } = "";

        /// <summary>The SAF tree uri that was picked for <see cref="GamePath"/>.</summary>
        public string TreeUri { get; set; } = "";

        public ResolutionPreset Resolution { get; set; } = ResolutionPreset.DeviceNative;

        public int CustomWidth { get; set; } = 1280;

        public int CustomHeight { get; set; } = 720;

        /// <summary>Enables the 3D world renderer.</summary>
        public bool Enable3D { get; set; } = true;

        public GraphicsMode Graphics { get; set; } = GraphicsMode.OpenGLEs3;

        public UiScaleOption UiScale { get; set; } = UiScaleOption.Automatic;

        /// <summary>Enables the haptic feedback of the on-screen controls.</summary>
        public bool Haptics { get; set; } = true;

        /// <summary>Loads the settings from the shared preferences.</summary>
        public static SimitoneAndroidSettings Load(Context context)
        {
            var prefs = AndroidGamePath.GetPreferences(context);
            var settings = new SimitoneAndroidSettings
            {
                GamePath = prefs.GetString(AndroidGamePath.KeyGamePath, "") ?? "",
                TreeUri = prefs.GetString(AndroidGamePath.KeyTreeUri, "") ?? "",
                CustomWidth = prefs.GetInt(AndroidGamePath.KeyCustomWidth, 1280),
                CustomHeight = prefs.GetInt(AndroidGamePath.KeyCustomHeight, 720),
                Enable3D = prefs.GetBoolean(AndroidGamePath.KeyEnable3D, true),
                Haptics = prefs.GetBoolean(AndroidGamePath.KeyHaptics, true)
            };

            var preset = prefs.GetString(AndroidGamePath.KeyResolutionPreset, ResolutionPreset.DeviceNative.ToString());
            ResolutionPreset parsedPreset;
            settings.Resolution = Enum.TryParse(preset, true, out parsedPreset) ? parsedPreset : ResolutionPreset.DeviceNative;

            var graphics = prefs.GetString(AndroidGamePath.KeyGraphicsMode, GraphicsMode.OpenGLEs3.ToString());
            GraphicsMode parsedGraphics;
            settings.Graphics = Enum.TryParse(graphics, true, out parsedGraphics) ? parsedGraphics : GraphicsMode.OpenGLEs3;

            var scale = prefs.GetString(AndroidGamePath.KeyUiScale, UiScaleOption.Automatic.ToString());
            UiScaleOption parsedScale;
            settings.UiScale = Enum.TryParse(scale, true, out parsedScale) ? parsedScale : UiScaleOption.Automatic;

            return settings;
        }

        /// <summary>Stores the settings in the shared preferences.</summary>
        public void Save(Context context)
        {
            var editor = AndroidGamePath.GetPreferences(context).Edit();
            editor.PutString(AndroidGamePath.KeyGamePath, GamePath ?? "");
            editor.PutString(AndroidGamePath.KeyTreeUri, TreeUri ?? "");
            editor.PutString(AndroidGamePath.KeyResolutionPreset, Resolution.ToString());
            editor.PutInt(AndroidGamePath.KeyCustomWidth, CustomWidth);
            editor.PutInt(AndroidGamePath.KeyCustomHeight, CustomHeight);
            editor.PutBoolean(AndroidGamePath.KeyEnable3D, Enable3D);
            editor.PutString(AndroidGamePath.KeyGraphicsMode, Graphics.ToString());
            editor.PutString(AndroidGamePath.KeyUiScale, UiScale.ToString());
            editor.PutBoolean(AndroidGamePath.KeyHaptics, Haptics);
            editor.Apply();
        }

        /// <summary>The names of the resolution presets, in the order they are shown.</summary>
        public static string[] ResolutionNames
        {
            get
            {
                return new[]
                {
                    "Device native resolution",
                    "1280 x 720",
                    "1600 x 900",
                    "1920 x 1080",
                    "2560 x 1440",
                    "Custom..."
                };
            }
        }

        /// <summary>The names of the graphics modes, in the order they are shown.</summary>
        public static string[] GraphicsNames
        {
            get
            {
                return new[]
                {
                    "OpenGL ES 3.0 (recommended)",
                    "OpenGL ES 2.0 (compatibility)"
                };
            }
        }

        /// <summary>The names of the UI scale options, in the order they are shown.</summary>
        public static string[] UiScaleNames
        {
            get
            {
                return new[]
                {
                    "Automatic (based on the screen density)",
                    "100 %",
                    "125 %",
                    "150 %",
                    "200 %"
                };
            }
        }

        /// <summary>
        /// Resolves the requested window size.
        /// </summary>
        /// <param name="deviceWidth">Width of the device in pixels.</param>
        /// <param name="deviceHeight">Height of the device in pixels.</param>
        public void ResolveResolution(int deviceWidth, int deviceHeight, out int width, out int height)
        {
            switch (Resolution)
            {
                case ResolutionPreset.W1280x720:
                    width = 1280; height = 720; break;
                case ResolutionPreset.W1600x900:
                    width = 1600; height = 900; break;
                case ResolutionPreset.W1920x1080:
                    width = 1920; height = 1080; break;
                case ResolutionPreset.W2560x1440:
                    width = 2560; height = 1440; break;
                case ResolutionPreset.Custom:
                    width = Math.Max(640, CustomWidth);
                    height = Math.Max(360, CustomHeight);
                    break;
                default:
                    // The native resolution of the device, but never more than
                    // 1920x1080 so that the game stays playable on huge screens.
                    width = Math.Max(640, deviceWidth);
                    height = Math.Max(360, deviceHeight);
                    if (width > 1920)
                    {
                        height = (int)Math.Round(height * (1920f / width));
                        width = 1920;
                    }
                    break;
            }
        }

        /// <summary>
        /// A human readable description of the UI scale option.
        /// </summary>
        public string UiScaleDescription
        {
            get
            {
                switch (UiScale)
                {
                    case UiScaleOption.X100: return "The panels use their normal size.";
                    case UiScaleOption.X125: return "The panels are enlarged by 25 %.";
                    case UiScaleOption.X150: return "The panels are enlarged by 50 %.";
                    case UiScaleOption.X200: return "The panels are doubled in size.";
                    default: return "The panel size is derived from the screen density.";
                }
            }
        }
    }
}
