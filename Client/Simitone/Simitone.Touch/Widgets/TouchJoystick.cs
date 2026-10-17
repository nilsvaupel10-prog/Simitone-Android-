using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Touch.Input;

namespace Simitone.Touch.Widgets
{
    /// <summary>
    /// A virtual thumb stick used to move the camera.
    /// </summary>
    /// <remarks>
    /// The stick reports a normalised vector (<see cref="Value"/>) in the range
    /// -1 .. 1. The service translates that vector into the camera actions
    /// (forward / back / left / right) so that the engine only ever sees plain
    /// keyboard input.
    /// </remarks>
    public class TouchJoystick : TouchWidget
    {
        private Vector2 knobOffset;
        private Vector2 center;
        private bool floating;

        /// <summary>Current stick position, -1 .. 1 on both axes.</summary>
        public Vector2 Value { get; private set; }

        /// <summary>Radius of the stick in pixels.</summary>
        public float Radius
        {
            get { return Math.Min(Bounds.Width, Bounds.Height) * 0.5f; }
        }

        /// <summary>Radius the knob can travel, in pixels.</summary>
        public float KnobTravel
        {
            get { return Math.Max(1f, Radius * 0.55f); }
        }

        /// <summary>
        /// When true the stick re-centres on the finger that pressed it, which
        /// makes it much easier to use on a phone.
        /// </summary>
        public bool Floating
        {
            get { return floating; }
            set { floating = value; }
        }

        public TouchJoystick()
        {
            Value = Vector2.Zero;
        }

        public TouchJoystick(string id) : this()
        {
            Id = id;
        }

        public override bool HitTest(Vector2 point)
        {
            if (!Enabled) return false;
            if (floating)
            {
                // A floating stick can be grabbed a little bit outside of its
                // own area, which makes it much easier to use on a phone
                // without stealing the touches of the game.
                var area = new Rectangle(
                    Bounds.X - (int)(Bounds.Width * 0.3f),
                    Bounds.Y - (int)(Bounds.Height * 0.3f),
                    (int)(Bounds.Width * 1.6f),
                    (int)(Bounds.Height * 1.6f));
                return area.Contains(point);
            }
            return Bounds.Contains(point);
        }

        public override void OnPressed(TouchContact contact)
        {
            IsPressed = true;
            HapticTick();
            center = floating ? contact.Position : new Vector2(Bounds.X + Bounds.Width * 0.5f, Bounds.Y + Bounds.Height * 0.5f);
            UpdateKnob(contact.Position);
        }

        public override void OnMoved(TouchContact contact)
        {
            if (!IsPressed) return;
            UpdateKnob(contact.Position);
        }

        public override void OnReleased(TouchContact contact)
        {
            IsPressed = false;
            knobOffset = Vector2.Zero;
            Value = Vector2.Zero;
        }

        private void UpdateKnob(Vector2 position)
        {
            var delta = position - center;
            if (delta.Length() > KnobTravel)
            {
                delta = delta.Normalize() * KnobTravel;
            }
            knobOffset = delta;

            var raw = knobOffset / KnobTravel;
            float deadZone = Context != null ? Context.StickDeadZone : 0.18f;
            if (raw.Length() < deadZone)
            {
                Value = Vector2.Zero;
            }
            else
            {
                // Re-map the dead zone so the stick still reaches the full range.
                float magnitude = (raw.Length() - deadZone) / (1f - deadZone);
                Value = raw.Normalize() * magnitude;
            }
        }

        public override void Reset()
        {
            base.Reset();
            knobOffset = Vector2.Zero;
            Value = Vector2.Zero;
        }

        public override void Draw(SpriteBatch batch, TouchWidgetContext context)
        {
            if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

            var theme = context.Theme;
            float alpha = context.GlobalOpacity * Layout.Opacity * (Enabled ? 1f : 0.35f);
            if (alpha <= 0.01f) return;

            var circle = TouchPrimitives.GetCircle(batch.GraphicsDevice);
            var ring = TouchPrimitives.GetRing(batch.GraphicsDevice);

            var baseCenter = IsPressed && floating
                ? center
                : new Vector2(Bounds.X + Bounds.Width * 0.5f, Bounds.Y + Bounds.Height * 0.5f);

            int baseSize = (int)Math.Max(2, Math.Min(Bounds.Width, Bounds.Height));
            var baseBounds = new Rectangle(
                (int)(baseCenter.X - baseSize * 0.5f),
                (int)(baseCenter.Y - baseSize * 0.5f),
                baseSize,
                baseSize);
            batch.Draw(circle, baseBounds, theme.StickBackground * alpha);

            int ringSize = Math.Max(2, (int)(baseSize * 0.04f));
            var ringBounds = new Rectangle(
                baseBounds.X - ringSize,
                baseBounds.Y - ringSize,
                baseBounds.Width + ringSize * 2,
                baseBounds.Height + ringSize * 2);
            batch.Draw(ring, ringBounds, theme.ButtonRing * alpha);

            int knobSize = (int)Math.Max(4, baseSize * 0.38f);
            var knobCenter = baseCenter + knobOffset;
            var knobBounds = new Rectangle(
                (int)(knobCenter.X - knobSize * 0.5f),
                (int)(knobCenter.Y - knobSize * 0.5f),
                knobSize,
                knobSize);
            batch.Draw(circle, knobBounds, theme.StickKnob * alpha);

            int knobRing = Math.Max(1, (int)(knobSize * 0.06f));
            var knobRingBounds = new Rectangle(
                knobBounds.X - knobRing,
                knobBounds.Y - knobRing,
                knobBounds.Width + knobRing * 2,
                knobBounds.Height + knobRing * 2);
            batch.Draw(ring, knobRingBounds, theme.StickKnobRing * alpha);
        }
    }
}
