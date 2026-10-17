using System;
using System.IO;
using Android.Graphics;
using FSO.Files;

namespace Simitone.Android.Platform
{
    /// <summary>
    /// Decodes the images of the game data without relying on System.Drawing
    /// (which is not available on Android).
    /// </summary>
    /// <remarks>
    /// FreeSO hands the raw bytes to <c>Texture2D.SetData</c>, which expects the
    /// pixels in R, G, B, A order. The Android bitmap API returns ARGB integers,
    /// so the channels are re-ordered here.
    /// </remarks>
    public static class AndroidBitmapReader
    {
        /// <summary>Reads an image stream into RGBA bytes.</summary>
        public static Tuple<byte[], int, int> Read(Stream stream)
        {
            if (stream == null) return null;

            using (var bitmap = BitmapFactory.DecodeStream(stream, null, new BitmapFactory.Options
            {
                InScaled = false,
                InDither = false,
                InJustDecodeBounds = false,
                InPurgeable = false,
                InInputShareable = true
            }))
            {
                if (bitmap == null) return null;

                int width = bitmap.Width;
                int height = bitmap.Height;
                if (width <= 0 || height <= 0) return null;

                var pixels = new int[width * height];
                bitmap.GetPixels(pixels, 0, width, 0, 0, width, height);

                var data = new byte[width * height * 4];
                for (int i = 0; i < pixels.Length; i++)
                {
                    int pixel = pixels[i];
                    int offset = i * 4;
                    data[offset] = (byte)((pixel >> 16) & 0xFF);       // R
                    data[offset + 1] = (byte)((pixel >> 8) & 0xFF);    // G
                    data[offset + 2] = (byte)(pixel & 0xFF);           // B
                    data[offset + 3] = (byte)((pixel >> 24) & 0xFF);  // A
                }

                return new Tuple<byte[], int, int>(data, width, height);
            }
        }
    }
}
