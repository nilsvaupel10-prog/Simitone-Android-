using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Simitone.Touch
{
    /// <summary>
    /// Abstraction over the text rendering used by the control system.
    /// </summary>
    /// <remarks>
    /// The touch module is deliberately independent of the engine's font stack, so
    /// the host (Simitone.Client) plugs in its own renderer. When no renderer is
    /// supplied, all text is silently skipped - the controls still work, they just
    /// do not have captions.
    /// </remarks>
    public interface ITouchTextRenderer
    {
        /// <summary>Measures a string in pixels.</summary>
        Vector2 Measure(string text);

        /// <summary>Draws a string with its top left corner at the given position.</summary>
        void Draw(SpriteBatch batch, string text, Vector2 position, Color color);
    }

    /// <summary>
    /// A renderer that draws nothing. Used when the host does not provide a font.
    /// </summary>
    public class NullTouchTextRenderer : ITouchTextRenderer
    {
        public static readonly NullTouchTextRenderer Instance = new NullTouchTextRenderer();

        public Vector2 Measure(string text)
        {
            // A rough approximation so that layouts still look sane without a font.
            return new Vector2((text ?? string.Empty).Length * 7, 12);
        }

        public void Draw(SpriteBatch batch, string text, Vector2 position, Color color)
        {
        }
    }
}
