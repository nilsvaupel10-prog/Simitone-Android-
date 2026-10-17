using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Touch.Icons;
using Simitone.Touch.Input;

namespace Simitone.Touch.Widgets
{
    /// <summary>
    /// Everything a widget needs while it is being updated or drawn.
    /// </summary>
    public class TouchWidgetContext
    {
        /// <summary>Seconds since the previous frame.</summary>
        public float DeltaTime;

        /// <summary>The platform touch backend.</summary>
        public ITouchInputBackend Backend;

        /// <summary>Haptic feedback.</summary>
        public IHapticService Haptics;

        /// <summary>Called when a control wants to hold an action down.</summary>
        public Action<TouchAction, bool> SetActionHeld;

        /// <summary>Called when a control is tapped (a momentary action).</summary>
        public Action<TouchAction> TriggerAction;

        /// <summary>The current theme.</summary>
        public TouchControlTheme Theme;

        /// <summary>The icon provider.</summary>
        public TouchIconProvider Icons;

        /// <summary>Optional font used to draw the labels.</summary>
        public SpriteFont Font;

        /// <summary>Optional text renderer (labels, editor captions).</summary>
        public ITouchTextRenderer TextRenderer;

        /// <summary>Global opacity of the overlay.</summary>
        public float GlobalOpacity = 1f;

        /// <summary>Dead zone of the virtual stick.</summary>
        public float StickDeadZone = 0.18f;

        public void Hold(TouchAction action, bool held)
        {
            if (SetActionHeld != null) SetActionHeld(action, held);
        }

        public void Trigger(TouchAction action)
        {
            if (TriggerAction != null) TriggerAction(action);
        }

        public void HapticTick()
        {
            if (Haptics != null) Haptics.LightTick(1f);
        }
    }

    /// <summary>
    /// Base class of every element of the on-screen control system.
    /// </summary>
    public abstract class TouchWidget
    {
        /// <summary>Stable id, also used as the key in the profile file.</summary>
        public string Id { get; set; }

        /// <summary>The action this control performs.</summary>
        public TouchAction Action { get; set; }

        /// <summary>Relative placement of the control.</summary>
        public TouchWidgetLayout Layout { get; set; }

        /// <summary>Resolved screen position of the control.</summary>
        public Rectangle Bounds { get; set; }

        /// <summary>True while the control is being pressed.</summary>
        public bool IsPressed { get; set; }

        /// <summary>Animated 0 - 1 value describing how far the control is "pushed in".</summary>
        public float PressAmount { get; set; }

        /// <summary>Id of the finger that currently owns this control (-1 = none).</summary>
        public int CapturedContactId { get; set; }

        /// <summary>Disabled controls are drawn faded and ignore input.</summary>
        public bool Enabled { get; set; }

        /// <summary>Optional caption drawn underneath the control.</summary>
        public string Label { get; set; }

        /// <summary>
        /// The context of the current frame. The service assigns this before the
        /// widget is updated or drawn.
        /// </summary>
        public TouchWidgetContext Context { get; set; }

        protected TouchWidget()
        {
            CapturedContactId = -1;
            Enabled = true;
            Layout = new TouchWidgetLayout();
        }

        /// <summary>Holds the action of this control down (or releases it).</summary>
        protected void Hold(TouchAction action, bool held)
        {
            if (Context != null) Context.Hold(action, held);
        }

        /// <summary>Fires the action of this control once.</summary>
        protected void Trigger(TouchAction action)
        {
            if (Context != null) Context.Trigger(action);
        }

        /// <summary>Requests a short haptic pulse.</summary>
        protected void HapticTick()
        {
            if (Context != null) Context.HapticTick();
        }

        /// <summary>True when the given point is inside the control.</summary>
        public virtual bool HitTest(Vector2 point)
        {
            if (!Enabled) return false;
            return Bounds.Contains(point);
        }

        /// <summary>Called when a finger goes down on this control.</summary>
        public virtual void OnPressed(TouchContact contact)
        {
        }

        /// <summary>Called when a finger that owns this control moves.</summary>
        public virtual void OnMoved(TouchContact contact)
        {
        }

        /// <summary>Called when the finger that owns this control lifts.</summary>
        public virtual void OnReleased(TouchContact contact)
        {
        }

        /// <summary>Called once per frame after all finger events have been processed.</summary>
        public virtual void Update(float deltaTime)
        {
            float target = IsPressed ? 1f : 0f;
            float speed = 14f;
            if (PressAmount < target)
                PressAmount = Math.Min(target, PressAmount + deltaTime * speed);
            else if (PressAmount > target)
                PressAmount = Math.Max(target, PressAmount - deltaTime * speed);
        }

        /// <summary>Draws the control.</summary>
        public abstract void Draw(SpriteBatch batch, TouchWidgetContext context);

        /// <summary>Resets the interaction state (used when the overlay is hidden).</summary>
        public virtual void Reset()
        {
            CapturedContactId = -1;
            IsPressed = false;
            PressAmount = 0f;
        }
    }
}
