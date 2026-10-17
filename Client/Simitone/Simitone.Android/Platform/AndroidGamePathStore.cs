using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.Provider;

namespace Simitone.Android.Platform
{
    /// <summary>
    /// The result of validating a folder that is supposed to contain
    /// "The Sims: Complete Collection".
    /// </summary>
    public class GamePathValidation
    {
        /// <summary>True when the folder looks like a Sims 1 installation.</summary>
        public bool IsValid { get; set; }

        /// <summary>A short, user readable summary.</summary>
        public string Message { get; set; } = "";

        /// <summary>The individual checks that were performed.</summary>
        public List<string> Details { get; } = new List<string>();

        /// <summary>The files/directories that were found.</summary>
        public List<string> Found { get; } = new List<string>();
    }

    /// <summary>
    /// Locates and validates the folder that holds the user's copy of
    /// "The Sims: Complete Collection".
    /// </summary>
    /// <remarks>
    /// <para>
    /// The folder is picked with the Storage Access Framework
    /// (<c>ACTION_OPEN_DOCUMENT_TREE</c>). Because FreeSO reads the game data
    /// through <see cref="System.IO"/>, the SAF tree is translated back into a
    /// real file system path. That works for the primary shared storage (and for
    /// removable volumes whose UUID is known to Android) - for everything else
    /// the user can type the path by hand.
    /// </para>
    /// </remarks>
    public static class AndroidGamePath
    {
        /// <summary>Name of the shared preferences file used by the launcher.</summary>
        public const string PreferencesName = "simitone_setup";

        // --- persisted keys ---------------------------------------------------
        public const string KeyGamePath = "game_path";
        public const string KeyTreeUri = "tree_uri";
        public const string KeyResolutionPreset = "resolution_preset";
        public const string KeyCustomWidth = "custom_width";
        public const string KeyCustomHeight = "custom_height";
        public const string KeyEnable3D = "enable_3d";
        public const string KeyUiScale = "ui_scale";
        public const string KeyGraphicsMode = "graphics_mode";
        public const string KeySetupComplete = "setup_complete";
        public const string KeyHaptics = "haptics";

        /// <summary>Returns the shared preferences of the app.</summary>
        public static ISharedPreferences GetPreferences(Context context)
        {
            return context.GetSharedPreferences(PreferencesName, FileCreationMode.Private);
        }

        /// <summary>The game folder that was selected by the user (may be empty).</summary>
        public static string GetStoredPath(Context context)
        {
            return GetPreferences(context).GetString(KeyGamePath, "") ?? "";
        }

        /// <summary>The SAF tree uri that was selected by the user (may be empty).</summary>
        public static string GetStoredTreeUri(Context context)
        {
            return GetPreferences(context).GetString(KeyTreeUri, "") ?? "";
        }

        /// <summary>True when the user already went through the setup.</summary>
        public static bool IsSetupComplete(Context context)
        {
            return GetPreferences(context).GetBoolean(KeySetupComplete, false);
        }

        /// <summary>Stores the setup so the launcher is skipped on the next start.</summary>
        public static void MarkSetupComplete(Context context)
        {
            var editor = GetPreferences(context).Edit();
            editor.PutBoolean(KeySetupComplete, true);
            editor.Apply();
        }

        /// <summary>Forgets the whole setup (used by "reset" in the launcher).</summary>
        public static void ClearSetup(Context context)
        {
            var editor = GetPreferences(context).Edit();
            editor.Clear();
            editor.Apply();
        }

        /// <summary>
        /// Translates a SAF tree uri into a file system path.
        /// </summary>
        /// <returns>The path, or an empty string when it could not be resolved.</returns>
        public static string ResolveTreeToPath(Context context, Uri treeUri)
        {
            if (treeUri == null) return "";

            string documentId;
            try
            {
                documentId = DocumentsContract.GetTreeDocumentId(treeUri);
            }
            catch (Exception)
            {
                documentId = null;
            }

            if (string.IsNullOrEmpty(documentId))
            {
                // Some providers use an opaque document id - try the "tree" part
                // of the uri instead.
                var segments = treeUri.PathSegments;
                if (segments != null && segments.Count > 0)
                {
                    var last = segments[segments.Count - 1];
                    try
                    {
                        documentId = Uri.Decode(last);
                    }
                    catch (Exception)
                    {
                        documentId = last;
                    }
                }
            }

            if (string.IsNullOrEmpty(documentId)) return "";

            int separator = documentId.IndexOf(':');
            string volumeId = separator >= 0 ? documentId.Substring(0, separator) : "primary";
            string relativePath = separator >= 0 ? documentId.Substring(separator + 1) : "";

            var root = ResolveVolumeRoot(context, volumeId);
            if (string.IsNullOrEmpty(root)) return "";

            var combined = string.IsNullOrEmpty(relativePath)
                ? root
                : Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

            return combined;
        }

        /// <summary>Returns the mount point of a SAF volume ("primary", a UUID, ...).</summary>
        public static string ResolveVolumeRoot(Context context, string volumeId)
        {
            if (string.IsNullOrEmpty(volumeId)) return null;
            if (string.Equals(volumeId, "primary", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
#pragma warning disable CS0618 // The modern alternative needs extra permissions.
                    var external = Android.OS.Environment.GetExternalStorageDirectory();
#pragma warning restore CS0618
                    if (external != null) return external.AbsolutePath;
                }
                catch (Exception)
                {
                    // fall through
                }
                return "/storage/emulated/0";
            }

            // Removable volumes are identified by their UUID in the SAF.
            try
            {
                var storageManager = context.GetSystemService(Context.StorageService) as Android.App.StorageManager;
                if (storageManager != null)
                {
                    var volumes = storageManager.StorageVolumes;
                    if (volumes != null)
                    {
                        foreach (var volume in volumes)
                        {
                            var uuid = volume.Uuid;
                            if (!string.IsNullOrEmpty(uuid) &&
                                string.Equals(uuid, volumeId, StringComparison.OrdinalIgnoreCase))
                            {
                                var directory = volume.GetDirectory();
                                if (directory != null) return directory.AbsolutePath;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // StorageManager is not available on every device - fall back below.
            }

            return "/storage/" + volumeId;
        }

        /// <summary>
        /// Checks whether the given folder contains a usable copy of The Sims 1.
        /// </summary>
        public static GamePathValidation Validate(string path)
        {
            var result = new GamePathValidation();

            if (string.IsNullOrWhiteSpace(path))
            {
                result.Message = "No folder has been selected yet.";
                return result;
            }

            if (!Directory.Exists(path))
            {
                result.Message = "The folder does not exist or cannot be read: " + path;
                return result;
            }

            var behaviour = Path.Combine(path, "GameData", "Behavior.iff");
            var gameplay = Path.Combine(path, "GameData", "Gameplay.iff");
            var objects = Path.Combine(path, "GameData", "Objects.iff");
            var archive = Path.Combine(path, "TS1Archive.iff");
            var downloads = Path.Combine(path, "Downloads");
            var music = Path.Combine(path, "Music");
            var sfx = Path.Combine(path, "SFX");

            if (File.Exists(behaviour)) result.Found.Add("GameData/Behavior.iff");
            if (File.Exists(gameplay)) result.Found.Add("GameData/Gameplay.iff");
            if (File.Exists(objects)) result.Found.Add("GameData/Objects.iff");
            if (File.Exists(archive)) result.Found.Add("TS1Archive.iff");
            if (Directory.Exists(downloads)) result.Found.Add("Downloads/");
            if (Directory.Exists(music)) result.Found.Add("Music/");
            if (Directory.Exists(sfx)) result.Found.Add("SFX/");

            result.Details.Add("GameData/Behavior.iff : " + (File.Exists(behaviour) ? "found" : "MISSING"));
            result.Details.Add("GameData/Gameplay.iff : " + (File.Exists(gameplay) ? "found" : "MISSING"));

            if (!File.Exists(behaviour))
            {
                result.Message = "This does not look like a The Sims 1 installation - " +
                                 "GameData/Behavior.iff is missing.";
                return result;
            }

            if (!File.Exists(gameplay))
            {
                result.Message = "GameData/Behavior.iff was found, but GameData/Gameplay.iff is missing. " +
                                 "The installation may be incomplete.";
                return result;
            }

            result.IsValid = true;
            result.Message = "The Sims 1 data was found in this folder.";
            return result;
        }
    }
}
