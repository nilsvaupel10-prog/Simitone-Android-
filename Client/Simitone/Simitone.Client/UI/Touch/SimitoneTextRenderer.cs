using System;
using FSO.UI.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSDFData;
using Simitone.Touch;

namespace Simitone.Client.UI.Touch
{
    /// <summary>
    /// Draws the captions of the on-screen controls with the vector font of the
    /// engine.
    /// </summary>
    /// <remarks>
    /// The engine renders its text with the MSDF font directly on the graphics
    /// device, which means it cannot be mixed with an open sprite batch. The
    /// renderer therefore closes the batch, draws the text and opens it again.
    /// </remarks>
    public class SimitoneTextRenderer : ITouchTextRenderer
    {
        /// <summary>Default size of the captions relative to the natural font size.</summary>
        public const float DefaultScale = 0.8f;

        private readonly MSDFFont font;
        private readonly float scale;

        public SimitoneTextRenderer(FieldFont fieldFont, float scale = DefaultScale)
        {
            if (fieldFont == null) throw new ArgumentNullException("fieldFont");
            font = new MSDFFont(fieldFont);
            this.scale = scale;
        }

        /// <summary>Measures a string in pixels.</summary>
        public Vector2 Measure(string text)
        {
            if (string.IsNullOrEmpty(text)) return Vector2.Zero;
            var size = font.MeasureString(text);
            return new Vector2(size.X * scale, size.Y * scale);
        }

        /// <summary>Draws a string with its top left corner at the given position.</summary>
        public void Draw(SpriteBatch batch, string text, Vector2 position, Color color)
        {
            if (batch == null || string.IsNullOrEmpty(text)) return;

            var device = batch.GraphicsDevice;
            var restore = false;

            try
            {
                // The MSDF font draws with the graphics device, so the sprite
                // batch has to be closed first.
                batch.End();
                restore = true;
            }
            catch (InvalidOperationException)
            {
                // The batch was not open - nothing to restore.
            }

            try
            {
                font.Draw(device, text, position, color, new Vector2(scale, scale), null);
            }
            finally
            {
                if (restore)
                {
                    try
                    {
                        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                    }
                    catch (InvalidOperationException)
                    {
                        // The batch could not be restarted - the caller will
                        // notice the missing draws.
                    }
                }
            }
        }
    }
}
