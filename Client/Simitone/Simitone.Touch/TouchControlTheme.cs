using Microsoft.Xna.Framework;

namespace Simitone.Touch
{
    /// <summary>
    /// All colours and metrics used to draw the on-screen controls.
    /// </summary>
    /// <remarks>
    /// The theme is a plain object without any static state so that several themes
    /// (light / dark / high contrast) can coexist and be swapped at runtime.
    /// </remarks>
    public class TouchControlTheme
    {
        /// <summary>Colour of the circular background of a button.</summary>
        public Color ButtonBackground = new Color(28, 30, 38, 210);

        /// <summary>Colour of the circular background while the button is pressed.</summary>
        public Color ButtonBackgroundPressed = new Color(72, 132, 210, 235);

        /// <summary>Colour of the icon of a button.</summary>
        public Color IconTint = new Color(240, 244, 255);

        /// <summary>Colour of the icon of a button while it is pressed.</summary>
        public Color IconTintPressed = Color.White;

        /// <summary>Colour of the ring around a button.</summary>
        public Color ButtonRing = new Color(255, 255, 255, 60);

        /// <summary>Colour of the ring around a pressed button.</summary>
        public Color ButtonRingPressed = new Color(255, 255, 255, 150);

        /// <summary>Background of the virtual stick.</summary>
        public Color StickBackground = new Color(24, 26, 34, 170);

        /// <summary>Background of the knob of the virtual stick.</summary>
        public Color StickKnob = new Color(235, 240, 255, 220);

        /// <summary>Ring of the knob of the virtual stick.</summary>
        public Color StickKnobRing = new Color(20, 22, 30, 120);

        /// <summary>Colour of the text underneath a control.</summary>
        public Color LabelColor = new Color(255, 255, 255, 220);

        /// <summary>Colour of the outline drawn around a control in edit mode.</summary>
        public Color EditOutline = new Color(120, 200, 255, 255);

        /// <summary>Colour of the outline of the currently selected control.</summary>
        public Color EditOutlineSelected = new Color(255, 214, 92, 255);

        /// <summary>Colour of the resize handle in edit mode.</summary>
        public Color EditHandle = new Color(255, 214, 92, 255);

        /// <summary>Background of the editor toolbar.</summary>
        public Color EditorPanel = new Color(18, 20, 26, 235);

        /// <summary>Colour of the text in the editor toolbar.</summary>
        public Color EditorText = new Color(235, 240, 255);

        /// <summary>Radius of a round button as a fraction of its size.</summary>
        public float ButtonRadiusFactor = 0.46f;

        /// <summary>Thickness of the button ring as a fraction of the button size.</summary>
        public float ButtonRingFactor = 0.045f;

        /// <summary>How quickly a button animates towards its pressed state (per second).</summary>
        public float PressAnimationSpeed = 14f;

        /// <summary>Creates the default (dark, translucent) theme.</summary>
        public static TouchControlTheme CreateDefault()
        {
            return new TouchControlTheme();
        }

        /// <summary>Creates a high contrast theme, useful on very bright screens.</summary>
        public static TouchControlTheme CreateHighContrast()
        {
            return new TouchControlTheme
            {
                ButtonBackground = new Color(0, 0, 0, 235),
                ButtonBackgroundPressed = new Color(255, 190, 40, 255),
                IconTint = new Color(255, 255, 255),
                IconTintPressed = new Color(0, 0, 0),
                ButtonRing = new Color(255, 255, 255, 160),
                ButtonRingPressed = new Color(0, 0, 0, 200),
                StickBackground = new Color(0, 0, 0, 200),
                StickKnob = new Color(255, 255, 255, 240),
                LabelColor = Color.White
            };
        }
    }
}
