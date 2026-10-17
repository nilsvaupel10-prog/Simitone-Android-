using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simitone.Touch.Icons
{
    /// <summary>
    /// Loads the control icons.
    /// </summary>
    /// <remarks>
    /// The icons are shipped as PNG files that are embedded into this assembly, so
    /// they are always available no matter how the game was installed. Users can
    /// still drop their own PNGs into a folder (see <see cref="IconDirectory"/>)
    /// to replace them - the file name has to match the icon name
    /// ("&lt;name&gt;.png").
    /// </remarks>
    public class TouchIconProvider : IDisposable
    {
        private const string EmbeddedPrefix = "Simitone.Touch.Resources.TouchIcons.";

        private readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Texture2D> overrides = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private Texture2D pixel;
        private bool disposed;

        /// <summary>Optional folder with user supplied replacement icons.</summary>
        public string IconDirectory { get; set; }

        /// <summary>All icons that could be loaded (for diagnostics).</summary>
        public IEnumerable<string> LoadedIcons
        {
            get { return textures.Keys; }
        }

        /// <summary>
        /// Loads every embedded icon. Missing icons are silently ignored - the
        /// overlay falls back to a procedurally drawn glyph.
        /// </summary>
        public void Load(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (pixel == null) pixel = new Texture2D(graphicsDevice, 1, 1);

            var assembly = typeof(TouchIconProvider).GetTypeInfo().Assembly;
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (!name.StartsWith(EmbeddedPrefix, StringComparison.Ordinal)) continue;
                var iconName = name.Substring(EmbeddedPrefix.Length);
                iconName = Path.GetFileNameWithoutExtension(iconName);

                using (var stream = assembly.GetManifestResourceStream(name))
                {
                    if (stream == null) continue;
                    try
                    {
                        using (var buffer = new MemoryStream())
                        {
                            stream.CopyTo(buffer);
                            buffer.Position = 0;
                            textures[iconName] = Texture2D.FromStream(graphicsDevice, buffer);
                        }
                    }
                    catch (Exception)
                    {
                        // A broken icon must never stop the game from starting.
                    }
                }
            }

            LoadOverrides(graphicsDevice);
        }

        private void LoadOverrides(GraphicsDevice graphicsDevice)
        {
            overrides.Clear();
            if (string.IsNullOrEmpty(IconDirectory) || !Directory.Exists(IconDirectory)) return;

            try
            {
                foreach (var file in Directory.GetFiles(IconDirectory, "*.png"))
                {
                    var iconName = Path.GetFileNameWithoutExtension(file);
                    using (var stream = File.OpenRead(file))
                    {
                        overrides[iconName] = Texture2D.FromStream(graphicsDevice, stream);
                    }
                }
            }
            catch (Exception)
            {
                // Ignore unreadable override folders.
            }
        }

        /// <summary>Returns the texture for an icon, or null when it does not exist.</summary>
        public Texture2D Get(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;
            Texture2D texture;
            if (overrides.TryGetValue(iconName, out texture)) return texture;
            if (textures.TryGetValue(iconName, out texture)) return texture;
            return null;
        }

        /// <summary>A 1x1 white texture, used to draw shapes.</summary>
        public Texture2D Pixel
        {
            get { return pixel; }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var texture in textures.Values) texture.Dispose();
            foreach (var texture in overrides.Values) texture.Dispose();
            textures.Clear();
            overrides.Clear();
            if (pixel != null)
            {
                pixel.Dispose();
                pixel = null;
            }
        }
    }
}
