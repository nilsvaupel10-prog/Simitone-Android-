using System;
using System.Collections.Generic;
using System.IO;
using Android.Content;

namespace Simitone.Android.Platform
{
    /// <summary>
    /// Unpacks the assets that FreeSO reads through <see cref="System.IO"/> into
    /// the private storage of the app.
    /// </summary>
    /// <remarks>
    /// The compiled content (.xnb) is read directly from the APK, but the loose
    /// files (UI scripts, city data, meshes, the CAS database, ...) need a real
    /// directory. The copy is skipped when a marker file with the same version
    /// is already present.
    /// </remarks>
    public static class AndroidContentInstaller
    {
        private const string MarkerFile = ".simitone-content-version";
        private const string ContentVersion = "1";

        /// <summary>Asset folder that holds the FreeSO content.</summary>
        public const string FreeSoAssetRoot = "FreeSO/Content";

        /// <summary>Asset folder that holds the Simitone content.</summary>
        public const string SimitoneAssetRoot = "Simitone/Content";

        /// <summary>
        /// Copies the loose content into <paramref name="targetRoot"/>.
        /// </summary>
        /// <returns>True when something was copied.</returns>
        public static bool Install(Context context, string targetRoot)
        {
            if (context == null) throw new ArgumentNullException("context");

            var contentRoot = Path.Combine(targetRoot, "Content");
            var marker = Path.Combine(targetRoot, MarkerFile);

            try
            {
                if (File.Exists(marker))
                {
                    var version = File.ReadAllText(marker);
                    if (version == ContentVersion && Directory.Exists(contentRoot)) return false;
                }
            }
            catch (Exception)
            {
                // Fall through and copy again.
            }

            CopyAssetFolder(context, FreeSoAssetRoot, contentRoot);
            // The Simitone content is copied afterwards so that it wins over the
            // FreeSO files with the same name.
            CopyAssetFolder(context, SimitoneAssetRoot, contentRoot);

            try
            {
                Directory.CreateDirectory(targetRoot);
                File.WriteAllText(marker, ContentVersion);
            }
            catch (Exception)
            {
                // The marker is only an optimisation.
            }

            return true;
        }

        private static void CopyAssetFolder(Context context, string assetFolder, string target)
        {
            string[] names;
            try
            {
                names = context.Assets.List(assetFolder);
            }
            catch (Exception)
            {
                return;
            }

            if (names == null) return;

            foreach (var name in names)
            {
                var childAsset = assetFolder + "/" + name;
                var childTarget = Path.Combine(target, name);

                if (name.Contains("."))
                {
                    // A file.
                    try
                    {
                        var parent = Path.GetDirectoryName(childTarget);
                        if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);

                        using (var input = context.Assets.Open(childAsset))
                        using (var output = File.Open(childTarget, FileMode.Create, FileAccess.Write))
                        {
                            input.CopyTo(output);
                        }
                    }
                    catch (Exception)
                    {
                        // Skip files that cannot be read.
                    }
                }
                else
                {
                    // A directory.
                    CopyAssetFolder(context, childAsset, childTarget);
                }
            }
        }
    }
}
