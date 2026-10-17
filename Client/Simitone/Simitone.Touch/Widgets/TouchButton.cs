using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Touch.Icons;

namespace Simitone.Touch.Widgets
{
    /// <summary>
    /// A round, pressable control.
    /// </summary>
    /// <remarks>
    /// A button can be
    /// <list type="bullet">
    ///   <item><description><b>momentary</b> - the action is held while the button is pressed
    ///   (used for the camera buttons),</description></item>
    ///   <item><description><b>tapped</b> - the action fires once when the button is released
    ///   (used for "pause" or "options"),</description></item>
    ///   <item><description><b>toggled</b> - the action switches between on and off
    ///   (used for the build mode button).</description></item>
    /// </list>
    /// </remarks>
    public class TouchButton : TouchWidget
    {
        /// <summary>How the button reports its action.</summary>
        public enum ButtonMode
        {
            /// <summary>The action is held down while the button is pressed.</summary>
            Hold,
            /// <summary>The action fires once when the button is pressed.</summary>
            Tap,
            /// <summary>The action toggles a persistent on/off state.</summary>
            Toggle
        }

        /// <summary>Name of the icon (without extension) that is drawn on the button.</summary>
        public string IconName { get; set; }

        /// <summary>How the button behaves.</summary>
        public ButtonMode Mode { get; set; }

        /// <summary>Current state of a toggle button.</summary>
        public bool ToggleState { get; set; }

        /// <summary>Called when a toggle button changes its state.</summary>
        public Action<TouchButton, bool> ToggleChanged;

        /// <summary>Additional scale applied while the button is pressed.</summary>
        public float PressedScaleFactor { get; set; } = 0.92f;

        public TouchButton()
        {
            Mode = ButtonMode.Hold;
        }

        public TouchButton(string id, TouchAction action, string iconName, ButtonMode mode) : this()
        {
            Id = id;
            Action = action;
            IconName = iconName;
            Mode = mode;
        }

        public override void OnPressed(TouchContact contact)
        {
            IsPressed = true;
            HapticTick();
            if (Mode == ButtonMode.Hold)
            {
                Hold(Action, true);
            }
            else if (Mode == ButtonMode.Tap)
            {
                Trigger(Action);
            }
            else
            {
                ToggleState = !ToggleState;
                if (ToggleChanged != null) ToggleChanged(this, ToggleState);
            }
        }

        public override void OnReleased(TouchContact contact)
        {
            IsPressed = false;
            if (Mode == ButtonMode.Hold)
            {
                Hold(Action, false);
            }
        }

        public override void Reset()
        {
            base.Reset();
            if (Mode == ButtonMode.Hold) Hold(Action, false);
        }

        public override void Draw(SpriteBatch batch, TouchWidgetContext context)
        {
            if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

            var theme = context.Theme;
            float alpha = context.GlobalOpacity * Layout.Opacity * (Enabled ? 1f : 0.35f);
            if (alpha <= 0.01f) return;

            var bounds = Bounds;
            float scale = 1f - PressAmount * (1f - PressedScaleFactor);
            if (scale != 1f)
            {
                var center = new Vector2(bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f);
                float width = bounds.Width * scale;
                float height = bounds.Height * scale;
                bounds = new Rectangle(
                    (int)(center.X - width * 0.5f),
                    (int)(center.Y - height * 0.5f),
                    (int)width,
                    (int)height);
            }

            var background = theme.ButtonBackground;
            var ring = theme.ButtonRing;
            var iconTint = theme.IconTint;

            if (Mode == ButtonMode.Toggle && ToggleState)
            {
                background = theme.ButtonBackgroundPressed;
                ring = theme.ButtonRingPressed;
                iconTint = theme.IconTintPressed;
            }
            else if (IsPressed)
            {
                background = theme.ButtonBackgroundPressed;
                ring = theme.ButtonRingPressed;
                iconTint = theme.IconTintPressed;
            }

            var circle = TouchPrimitives.GetCircle(batch.GraphicsDevice);
            var ringTexture = TouchPrimitives.GetRing(batch.GraphicsDevice);

            batch.Draw(circle, bounds, background * alpha);

            int ringSize = Math.Max(2, (int)(bounds.Width * theme.ButtonRingFactor));
            var ringBounds = new Rectangle(
                bounds.X - ringSize,
                bounds.Y - ringSize,
                bounds.Width + ringSize * 2,
                bounds.Height + ringSize * 2);
            batch.Draw(ringTexture, ringBounds, ring * alpha);

            var icon = context.Icons != null ? context.Icons.Get(IconName) : null;
            if (icon != null)
            {
                float iconSize = bounds.Width * 0.52f;
                var iconBounds = new Rectangle(
                    (int)(bounds.X + (bounds.Width - iconSize) * 0.5f),
                    (int)(bounds.Y + (bounds.Height - iconSize) * 0.5f),
                    (int)iconSize,
                    (int)iconSize);
                batch.Draw(icon, iconBounds, iconTint * alpha);
            }
            else
            {
                // Fallback: a small dot so the control is never invisible.
                int dot = Math.Max(3, bounds.Width / 6);
                var dotBounds = new Rectangle(
                    bounds.X + (bounds.Width - dot) / 2,
                    bounds.Y + (bounds.Height - dot) / 2,
                    dot,
                    dot);
                batch.Draw(circle, dotBounds, iconTint * alpha);
            }
        }
    }
}
