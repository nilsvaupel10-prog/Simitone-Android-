using System;
using Microsoft.Xna.Framework;

namespace Simitone.Touch
{
    /// <summary>
    /// The corner of the screen a widget is anchored to.
    /// </summary>
    public enum TouchAnchor
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }

    /// <summary>
    /// Serializable placement of a single control.
    /// </summary>
    /// <remarks>
    /// All measurements are stored relative to the screen so that a layout created
    /// on a 1080p phone still looks right on a tablet:
    /// <list type="bullet">
    ///   <item><description><see cref="OffsetX"/> is a fraction of the screen <b>width</b>.</description></item>
    ///   <item><description><see cref="OffsetY"/> is a fraction of the screen <b>height</b>.</description></item>
    ///   <item><description><see cref="Width"/> / <see cref="Height"/> are fractions of the screen <b>height</b>,
    ///   which keeps round controls round on any aspect ratio.</description></item>
    /// </list>
    /// </remarks>
    public class TouchWidgetLayout
    {
        /// <summary>Stable identifier of the widget (also the key used in the profile file).</summary>
        public string Id { get; set; }

        /// <summary>The corner the widget is positioned relative to.</summary>
        public TouchAnchor Anchor { get; set; }

        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        /// <summary>0 - 1. Additional transparency of this single control.</summary>
        public float Opacity { get; set; }

        /// <summary>False hides the control completely (it is also ignored for hit testing).</summary>
        public bool Visible { get; set; }

        /// <summary>Optional per widget scale factor (1 = default).</summary>
        public float Scale { get; set; }

        public TouchWidgetLayout()
        {
            Opacity = 1f;
            Visible = true;
            Scale = 1f;
            Anchor = TouchAnchor.BottomLeft;
        }

        public TouchWidgetLayout(string id) : this()
        {
            Id = id;
        }

        public TouchWidgetLayout Clone()
        {
            return new TouchWidgetLayout
            {
                Id = Id,
                Anchor = Anchor,
                OffsetX = OffsetX,
                OffsetY = OffsetY,
                Width = Width,
                Height = Height,
                Opacity = Opacity,
                Visible = Visible,
                Scale = Scale
            };
        }

        /// <summary>Resolves the stored, relative placement into screen pixels.</summary>
        public Rectangle Resolve(int screenWidth, int screenHeight, float globalScale)
        {
            float scale = globalScale * (Scale <= 0 ? 1f : Scale);
            float width = Math.Max(1f, Width * screenHeight * scale);
            float height = Math.Max(1f, Height * screenHeight * scale);

            float x;
            float y;
            switch (Anchor)
            {
                case TouchAnchor.TopLeft:
                    x = OffsetX * screenWidth;
                    y = OffsetY * screenHeight;
                    break;
                case TouchAnchor.TopRight:
                    x = screenWidth - OffsetX * screenWidth - width;
                    y = OffsetY * screenHeight;
                    break;
                case TouchAnchor.BottomLeft:
                    x = OffsetX * screenWidth;
                    y = screenHeight - OffsetY * screenHeight - height;
                    break;
                case TouchAnchor.BottomRight:
                    x = screenWidth - OffsetX * screenWidth - width;
                    y = screenHeight - OffsetY * screenHeight - height;
                    break;
                default:
                    x = screenWidth * 0.5f - width * 0.5f + OffsetX * screenWidth;
                    y = screenHeight * 0.5f - height * 0.5f + OffsetY * screenHeight;
                    break;
            }

            return new Rectangle((int)Math.Round(x), (int)Math.Round(y), (int)Math.Round(width), (int)Math.Round(height));
        }

        /// <summary>Snaps the given rectangle to a grid of <paramref name="cell"/> pixels.</summary>
        public void SnapToGrid(Rectangle bounds, int cell)
        {
            if (cell <= 1) return;
            var snapped = new Rectangle(
                (int)Math.Round(bounds.X / (float)cell) * cell,
                (int)Math.Round(bounds.Y / (float)cell) * cell,
                bounds.Width,
                bounds.Height);
            ApplyBounds(snapped, bounds.Width, bounds.Height);
        }

        /// <summary>Writes an absolute pixel rectangle back into the relative layout.</summary>
        public void ApplyBounds(Rectangle bounds, int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0) return;
            Width = bounds.Width / (float)screenHeight;
            Height = bounds.Height / (float)screenHeight;

            switch (Anchor)
            {
                case TouchAnchor.TopLeft:
                    OffsetX = bounds.X / (float)screenWidth;
                    OffsetY = bounds.Y / (float)screenHeight;
                    break;
                case TouchAnchor.TopRight:
                    OffsetX = (screenWidth - bounds.Right) / (float)screenWidth;
                    OffsetY = bounds.Y / (float)screenHeight;
                    break;
                case TouchAnchor.BottomLeft:
                    OffsetX = bounds.X / (float)screenWidth;
                    OffsetY = (screenHeight - bounds.Bottom) / (float)screenHeight;
                    break;
                case TouchAnchor.BottomRight:
                    OffsetX = (screenWidth - bounds.Right) / (float)screenWidth;
                    OffsetY = (screenHeight - bounds.Bottom) / (float)screenHeight;
                    break;
                default:
                    OffsetX = (bounds.X + bounds.Width * 0.5f - screenWidth * 0.5f) / (float)screenWidth;
                    OffsetY = (bounds.Y + bounds.Height * 0.5f - screenHeight * 0.5f) / (float)screenHeight;
                    break;
            }
        }
    }
}
