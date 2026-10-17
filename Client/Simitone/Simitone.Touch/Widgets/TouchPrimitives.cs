using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simitone.Touch.Widgets
{
    /// <summary>
    /// Small helper that generates the primitive shapes (circle, ring, rounded
    /// rectangle) used by the control system.
    /// </summary>
    /// <remarks>
    /// Generating them at runtime keeps the module free of content pipeline
    /// dependencies - the overlay works even when no content has been built yet.
    /// </remarks>
    public static class TouchPrimitives
    {
        private const int Resolution = 128;

        private static readonly Dictionary<GraphicsDevice, Texture2D> Circles = new Dictionary<GraphicsDevice, Texture2D>();
        private static readonly Dictionary<GraphicsDevice, Texture2D> Rings = new Dictionary<GraphicsDevice, Texture2D>();

        /// <summary>A white circle with a soft edge, 128x128.</summary>
        public static Texture2D GetCircle(GraphicsDevice device)
        {
            Texture2D texture;
            if (Circles.TryGetValue(device, out texture) && texture != null && !texture.IsDisposed) return texture;
            texture = Build(device, false);
            Circles[device] = texture;
            return texture;
        }

        /// <summary>A white ring with a soft edge, 128x128.</summary>
        public static Texture2D GetRing(GraphicsDevice device)
        {
            Texture2D texture;
            if (Rings.TryGetValue(device, out texture) && texture != null && !texture.IsDisposed) return texture;
            texture = Build(device, true);
            Rings[device] = texture;
            return texture;
        }

        private static Texture2D Build(GraphicsDevice device, bool ring)
        {
            var texture = new Texture2D(device, Resolution, Resolution, false, SurfaceFormat.Color);
            var data = new Color[Resolution * Resolution];
            float center = (Resolution - 1) * 0.5f;

            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    float alpha;
                    if (ring)
                    {
                        // Ring with an inner radius of 0.78 and an outer radius of 1.0.
                        float outer = 1f - SmoothStep(0.92f, 1.0f, distance);
                        float inner = SmoothStep(0.70f, 0.80f, distance);
                        alpha = outer * inner;
                    }
                    else
                    {
                        alpha = 1f - SmoothStep(0.88f, 1.0f, distance);
                    }

                    data[y * Resolution + x] = new Color((byte)255, (byte)255, (byte)255, (byte)Math.Max(0, Math.Min(255, alpha * 255)));
                }
            }

            texture.SetData(data);
            return texture;
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            if (edge1 <= edge0) return value < edge0 ? 0f : 1f;
            float t = (value - edge0) / (edge1 - edge0);
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;
            return t * t * (3f - 2f * t);
        }

        /// <summary>Releases every cached texture (call when the graphics device is reset).</summary>
        public static void Clear()
        {
            foreach (var texture in Circles.Values) texture.Dispose();
            foreach (var texture in Rings.Values) texture.Dispose();
            Circles.Clear();
            Rings.Clear();
        }
    }
}
