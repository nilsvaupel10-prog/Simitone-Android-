using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Simitone.Touch.Editor;
using Simitone.Touch.Icons;
using Simitone.Touch.Input;
using Simitone.Touch.Widgets;

namespace Simitone.Touch
{
    /// <summary>
    /// The on-screen control system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The service owns the widgets, processes the fingers, keeps track of which
    /// actions are currently held down and renders the overlay. It has no
    /// dependency on the game engine: the host reads <see cref="GetVirtualKeys"/>
    /// and <see cref="TappedKeys"/> and injects them into the regular input
    /// pipeline, and listens to <see cref="ActionTriggered"/> for the actions that
    /// have no keyboard equivalent.
    /// </para>
    /// <para>
    /// Typical usage:
    /// <code>
    /// var touch = new TouchControlService(GraphicsDevice);
    /// touch.LoadProfile(profile);
    /// // every frame
    /// touch.Update(deltaTime);
    /// state.KeyboardState = new KeyboardState(touch.GetVirtualKeys());
    /// // ... and when drawing
    /// touch.Draw();
    /// </code>
    /// </para>
    /// </remarks>
    public class TouchControlService : IDisposable
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly ITouchInputBackend backend;
        private readonly TouchIconProvider icons;
        private readonly List<TouchWidget> widgets = new List<TouchWidget>();
        private readonly List<TouchWidget> drawOrder = new List<TouchWidget>();
        private readonly Dictionary<TouchAction, bool> heldActions = new Dictionary<TouchAction, bool>();
        private readonly List<Keys> virtualKeys = new List<Keys>();
        private readonly List<Keys> tappedKeys = new List<Keys>();
        private readonly HashSet<int> consumedTouches = new HashSet<int>();
        private readonly TouchLayoutEditor editor;
        private readonly TouchWidgetContext context;

        private SpriteBatch batch;
        private bool disposed;

        // --- gesture state -----------------------------------------------------
        private int gestureTouchA = -1;
        private int gestureTouchB = -1;
        private float gestureLastDistance;
        private float gestureLastAngle;
        private Vector2 gestureLastMidpoint;
        private int zoomHoldFrames;
        private TouchAction zoomHoldAction = TouchAction.None;
        private int rotateHoldFrames;
        private TouchAction rotateHoldAction = TouchAction.None;
        private int panHoldFrames;
        private TouchAction panHoldAction = TouchAction.None;

        /// <summary>Raised whenever an action is triggered (tapped or toggled).</summary>
        public event Action<TouchAction> ActionTriggered;

        /// <summary>Raised when the user toggles a two state control.</summary>
        public event Action<TouchButton, bool> ToggleChanged;

        /// <summary>All widgets of the current profile.</summary>
        public IList<TouchWidget> Widgets
        {
            get { return widgets; }
        }

        /// <summary>The profile that is currently loaded.</summary>
        public TouchControlProfile Profile { get; private set; }

        /// <summary>Settings of the current profile.</summary>
        public TouchControlSettings Settings
        {
            get { return Profile != null ? Profile.Settings : new TouchControlSettings(); }
        }

        /// <summary>The theme used to draw the controls.</summary>
        public TouchControlTheme Theme { get; set; }

        /// <summary>The icon provider (exposed so that users can add their own icons).</summary>
        public TouchIconProvider Icons
        {
            get { return icons; }
        }

        /// <summary>Optional text renderer supplied by the host.</summary>
        public ITouchTextRenderer TextRenderer { get; set; }

        /// <summary>Haptic feedback.</summary>
        public IHapticService Haptics { get; set; }

        /// <summary>When false the overlay is invisible and does not react to input.</summary>
        public bool Enabled { get; set; }

        /// <summary>When true the layout editor is shown instead of the normal controls.</summary>
        public bool EditMode
        {
            get { return editor != null && editor.Active; }
            set
            {
                if (editor == null) return;
                editor.Active = value;
                if (value) ResetWidgetStates();
            }
        }

        /// <summary>True while the layout editor is open.</summary>
        public bool IsEditing
        {
            get { return EditMode; }
        }

        /// <summary>The ids of the touches that were swallowed by the overlay.</summary>
        public IEnumerable<int> ConsumedTouchIds
        {
            get { return consumedTouches; }
        }

        /// <summary>The keys that should be reported as "pressed" this frame.</summary>
        public Keys[] GetVirtualKeys()
        {
            return virtualKeys.ToArray();
        }

        /// <summary>The keys that were tapped this frame (press and release).</summary>
        public IList<Keys> TappedKeys
        {
            get { return tappedKeys; }
        }

        /// <summary>True when at least one action is currently held down.</summary>
        public bool AnyActionHeld
        {
            get { return heldActions.Values.Any(x => x); }
        }

        public TouchControlService(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, new MonoGameTouchBackend(), NullHapticService.Instance)
        {
        }

        public TouchControlService(GraphicsDevice graphicsDevice, ITouchInputBackend backend, IHapticService haptics)
        {
            if (graphicsDevice == null) throw new ArgumentNullException("graphicsDevice");
            if (backend == null) throw new ArgumentNullException("backend");

            this.graphicsDevice = graphicsDevice;
            this.backend = backend;
            this.Haptics = haptics ?? NullHapticService.Instance;
            this.Theme = TouchControlTheme.CreateDefault();
            this.Enabled = true;

            icons = new TouchIconProvider();
            icons.Load(graphicsDevice);

            batch = new SpriteBatch(graphicsDevice);

            // The context is shared by all widgets. It carries the callbacks
            // that turn a press of a control into an action of the service.
            context = new TouchWidgetContext
            {
                Backend = backend,
                Haptics = this.Haptics,
                Theme = Theme,
                Icons = icons,
                SetActionHeld = SetActionHeld,
                TriggerAction = TriggerAction
            };

            Profile = TouchControlFactory.CreateDefaultProfile();
            RebuildWidgets();

            editor = new TouchLayoutEditor(this);
        }

        /// <summary>
        /// Copies the current settings of the service into the context that the
        /// widgets use.
        /// </summary>
        private void RefreshContext(float deltaTime)
        {
            context.Backend = backend;
            context.Haptics = Haptics;
            context.Theme = Theme;
            context.Icons = icons;
            context.TextRenderer = TextRenderer;
            context.GlobalOpacity = Settings.Opacity;
            context.StickDeadZone = Settings.StickDeadZone;
            context.DeltaTime = deltaTime;
        }

        /// <summary>Loads a profile and rebuilds the widgets.</summary>
        public void LoadProfile(TouchControlProfile profile)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            Profile = profile;
            if (Profile.Settings == null) Profile.Settings = new TouchControlSettings();
            Profile.Settings.Sanitize();
            RebuildWidgets();
        }

        /// <summary>Restores the factory default layout.</summary>
        public void ResetToDefault()
        {
            LoadProfile(TouchControlFactory.CreateDefaultProfile());
        }

        /// <summary>Rebuilds the widget list from the current profile.</summary>
        public void RebuildWidgets()
        {
            widgets.Clear();
            drawOrder.Clear();
            heldActions.Clear();

            var created = TouchControlFactory.CreateWidgets(Profile);
            foreach (var widget in created)
            {
                widgets.Add(widget);
                drawOrder.Add(widget);
            }

            // Apply the saved placement.
            foreach (var widget in widgets)
            {
                TouchWidgetLayout layout;
                if (Profile.Widgets.TryGetValue(widget.Id, out layout) && layout != null)
                {
                    widget.Layout = layout;
                }
                else
                {
                    Profile.Widgets[widget.Id] = widget.Layout;
                }
            }

            // Apply the draw order.
            if (Profile.Order != null && Profile.Order.Count > 0)
            {
                var ordered = new List<TouchWidget>();
                foreach (var id in Profile.Order)
                {
                    var widget = widgets.FirstOrDefault(x => x.Id == id);
                    if (widget != null && !ordered.Contains(widget)) ordered.Add(widget);
                }
                foreach (var widget in widgets)
                {
                    if (!ordered.Contains(widget)) ordered.Add(widget);
                }
                drawOrder.Clear();
                drawOrder.AddRange(ordered);
            }
        }

        /// <summary>
        /// Copies the current placement of every widget back into the profile so
        /// that it can be saved.
        /// </summary>
        public void StoreLayoutIntoProfile()
        {
            foreach (var widget in widgets)
            {
                Profile.Widgets[widget.Id] = widget.Layout;
            }
            Profile.Order = drawOrder.Select(x => x.Id).ToList();
        }

        /// <summary>Resolves the on-screen position of every widget.</summary>
        public void UpdateLayout(int screenWidth, int screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            float scale = Settings.Scale;
            foreach (var widget in widgets)
            {
                widget.Bounds = widget.Layout.Resolve(screenWidth, screenHeight, scale);
            }
        }

        /// <summary>Width of the screen the layout was last resolved for.</summary>
        public int ScreenWidth { get; private set; }

        /// <summary>Height of the screen the layout was last resolved for.</summary>
        public int ScreenHeight { get; private set; }

        /// <summary>Processes the fingers and updates all widgets.</summary>
        public void Update(float deltaTime)
        {
            tappedKeys.Clear();
            consumedTouches.Clear();

            if (!Enabled)
            {
                ResetWidgetStates();
                virtualKeys.Clear();
                return;
            }

            RefreshContext(deltaTime);

            // The widgets need the context before they are updated, because
            // that is where they report their actions.
            for (int i = 0; i < widgets.Count; i++)
            {
                widgets[i].Context = context;
            }

            backend.BeginFrame();
            var contacts = backend.GetContacts();

            if (editor != null && editor.Active)
            {
                editor.Update(contacts, deltaTime);
                // While editing, every touch belongs to the editor.
                foreach (var contact in contacts) consumedTouches.Add(contact.Id);
                virtualKeys.Clear();
                backend.EndFrame();
                return;
            }

            UpdateGestures(contacts);
            DispatchContacts(contacts);

            foreach (var widget in widgets)
            {
                widget.Update(deltaTime);
            }

            RebuildVirtualKeys();
            backend.EndFrame();
        }

        private void DispatchContacts(IList<TouchContact> contacts)
        {
            // 1) new presses
            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                if (contact.State != TouchContactState.Pressed) continue;

                var widget = FindWidgetAt(contact.Position);
                if (widget == null) continue;

                widget.CapturedContactId = contact.Id;
                widget.IsPressed = true;
                consumedTouches.Add(contact.Id);
                widget.OnPressed(contact);
            }

            // 2) movement
            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                if (contact.State != TouchContactState.Moved) continue;

                var widget = FindOwner(contact.Id);
                if (widget == null) continue;
                widget.OnMoved(contact);
            }

            // 3) releases
            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                if (contact.State != TouchContactState.Released) continue;

                var widget = FindOwner(contact.Id);
                if (widget != null)
                {
                    widget.OnReleased(contact);
                    widget.CapturedContactId = -1;
                    widget.IsPressed = false;
                }
            }
        }

        private TouchWidget FindWidgetAt(Vector2 position)
        {
            // Later widgets are drawn on top, so they get the touch first.
            for (int i = drawOrder.Count - 1; i >= 0; i--)
            {
                var widget = drawOrder[i];
                if (!widget.Layout.Visible) continue;
                if (widget.HitTest(position)) return widget;
            }
            return null;
        }

        private TouchWidget FindOwner(int contactId)
        {
            if (contactId < 0) return null;
            for (int i = 0; i < widgets.Count; i++)
            {
                if (widgets[i].CapturedContactId == contactId) return widgets[i];
            }
            return null;
        }

        private void ResetWidgetStates()
        {
            foreach (var widget in widgets)
            {
                widget.Reset();
            }
            foreach (var pair in heldActions.ToList())
            {
                heldActions[pair.Key] = false;
            }
            virtualKeys.Clear();
            tappedKeys.Clear();
        }

        /// <summary>Holds or releases an action.</summary>
        public void SetActionHeld(TouchAction action, bool held)
        {
            if (action == TouchAction.None) return;
            bool current;
            heldActions.TryGetValue(action, out current);
            if (current == held) return;
            heldActions[action] = held;
        }

        /// <summary>True when the given action is currently held down.</summary>
        public bool IsActionHeld(TouchAction action)
        {
            bool current;
            return heldActions.TryGetValue(action, out current) && current;
        }

        /// <summary>Fires an action (tapped) and raises <see cref="ActionTriggered"/>.</summary>
        public void TriggerAction(TouchAction action)
        {
            if (action == TouchAction.None) return;

            var descriptor = TouchActionCatalog.Get(action);
            foreach (var key in descriptor.TappedKeys)
            {
                if (!tappedKeys.Contains(key)) tappedKeys.Add(key);
            }

            if (ActionTriggered != null) ActionTriggered(action);
        }

        private void RebuildVirtualKeys()
        {
            virtualKeys.Clear();

            // The virtual stick maps to the four camera directions.
            var stick = widgets.OfType<TouchJoystick>().FirstOrDefault(x => x.Layout.Visible);
            if (stick != null && stick.Value != Vector2.Zero)
            {
                float threshold = 0.45f;
                if (stick.Value.Y < -threshold) SetActionHeld(TouchAction.CameraForward, true);
                else if (stick.Value.Y > threshold) SetActionHeld(TouchAction.CameraBack, true);

                if (stick.Value.X < -threshold) SetActionHeld(TouchAction.CameraLeft, true);
                else if (stick.Value.X > threshold) SetActionHeld(TouchAction.CameraRight, true);
            }
            else if (stick != null)
            {
                SetActionHeld(TouchAction.CameraForward, false);
                SetActionHeld(TouchAction.CameraBack, false);
                SetActionHeld(TouchAction.CameraLeft, false);
                SetActionHeld(TouchAction.CameraRight, false);
            }

            // Gesture driven actions are held for a couple of frames.
            ApplyTimedHold(ref zoomHoldFrames, ref zoomHoldAction, TouchAction.ZoomIn, TouchAction.ZoomOut);
            ApplyTimedHold(ref rotateHoldFrames, ref rotateHoldAction, TouchAction.RotateLeft, TouchAction.RotateRight);
            ApplyTimedHold(ref panHoldFrames, ref panHoldAction, TouchAction.CameraForward, TouchAction.CameraBack);

            foreach (var pair in heldActions)
            {
                if (!pair.Value) continue;
                var descriptor = TouchActionCatalog.Get(pair.Key);
                foreach (var key in descriptor.HeldKeys)
                {
                    if (!virtualKeys.Contains(key)) virtualKeys.Add(key);
                }
            }

            // Tapped keys are reported for exactly one frame.
            foreach (var key in tappedKeys)
            {
                if (!virtualKeys.Contains(key)) virtualKeys.Add(key);
            }
        }

        private void ApplyTimedHold(ref int frames, ref TouchAction action, TouchAction positive, TouchAction negative)
        {
            if (frames <= 0) return;
            if (action == positive) SetActionHeld(positive, true);
            else if (action == negative) SetActionHeld(negative, true);
            frames--;
        }

        #region gestures

        private void UpdateGestures(IList<TouchContact> contacts)
        {
            var settings = Settings;

            // Collect the fingers that are not used by a widget.
            var free = new List<TouchContact>();
            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                if (contact.State == TouchContactState.Released) continue;
                if (FindOwner(contact.Id) != null) continue;
                if (consumedTouches.Contains(contact.Id)) continue;
                free.Add(contact);
            }

            if (free.Count < 2)
            {
                gestureTouchA = -1;
                gestureTouchB = -1;
                return;
            }

            var a = free[0];
            var b = free[1];
            if (gestureTouchA != a.Id || gestureTouchB != b.Id)
            {
                gestureTouchA = a.Id;
                gestureTouchB = b.Id;
                var initial = b.Position - a.Position;
                gestureLastDistance = Math.Max(1f, initial.Length());
                gestureLastAngle = (float)Math.Atan2(initial.Y, initial.X);
                gestureLastMidpoint = (a.Position + b.Position) * 0.5f;
                return;
            }

            var vector = b.Position - a.Position;
            float distance = Math.Max(1f, vector.Length());
            float angle = (float)Math.Atan2(vector.Y, vector.X);
            var midpoint = (a.Position + b.Position) * 0.5f;

            if (settings.PinchToZoom)
            {
                float delta = distance - gestureLastDistance;
                const float threshold = 12f;
                if (Math.Abs(delta) >= threshold)
                {
                    int frames = Math.Max(1, (int)(Math.Abs(delta) / threshold));
                    if (delta > 0) { zoomHoldAction = TouchAction.ZoomIn; }
                    else { zoomHoldAction = TouchAction.ZoomOut; }
                    zoomHoldFrames = Math.Max(zoomHoldFrames, frames);
                }
            }

            if (settings.TwoFingerRotate)
            {
                float angleDelta = angle - gestureLastAngle;
                while (angleDelta > Math.PI) angleDelta -= (float)(Math.PI * 2);
                while (angleDelta < -Math.PI) angleDelta += (float)(Math.PI * 2);
                const float threshold = 0.12f;
                if (Math.Abs(angleDelta) >= threshold)
                {
                    int frames = Math.Max(1, (int)(Math.Abs(angleDelta) / threshold));
                    // Clockwise (screen coordinates, y grows downwards) rotates right.
                    if (angleDelta > 0) rotateHoldAction = TouchAction.RotateRight;
                    else rotateHoldAction = TouchAction.RotateLeft;
                    rotateHoldFrames = Math.Max(rotateHoldFrames, frames);
                }
            }

            if (settings.DragToPan)
            {
                var delta = midpoint - gestureLastMidpoint;
                const float threshold = 10f;
                if (Math.Abs(delta.X) >= threshold || Math.Abs(delta.Y) >= threshold)
                {
                    int frames = Math.Max(1, (int)(delta.Length() / threshold));
                    if (Math.Abs(delta.X) > Math.Abs(delta.Y))
                    {
                        panHoldAction = delta.X > 0 ? TouchAction.CameraRight : TouchAction.CameraLeft;
                    }
                    else
                    {
                        panHoldAction = delta.Y > 0 ? TouchAction.CameraBack : TouchAction.CameraForward;
                    }
                    panHoldFrames = Math.Max(panHoldFrames, frames);
                }
            }

            gestureLastDistance = distance;
            gestureLastAngle = angle;
            gestureLastMidpoint = midpoint;
        }

        #endregion

        /// <summary>Draws the overlay.</summary>
        public void Draw()
        {
            if (!Enabled) return;

            RefreshContext(context.DeltaTime);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            try
            {
                for (int i = 0; i < drawOrder.Count; i++)
                {
                    var widget = drawOrder[i];
                    if (!widget.Layout.Visible) continue;
                    widget.Context = context;
                    widget.Draw(batch, context);
                    DrawLabel(batch, context, widget);
                }

                if (editor != null && editor.Active)
                {
                    editor.Draw(batch, context);
                }
            }
            finally
            {
                batch.End();
            }
        }

        private void DrawLabel(SpriteBatch batch, TouchWidgetContext context, TouchWidget widget)
        {
            if (!Settings.ShowLabels) return;
            if (TextRenderer == null) return;

            string label = widget.Label;
            if (string.IsNullOrEmpty(label))
            {
                var descriptor = TouchActionCatalog.Get(widget.Action);
                label = descriptor.DisplayName;
            }
            if (string.IsNullOrEmpty(label)) return;

            float alpha = context.GlobalOpacity * widget.Layout.Opacity;
            var size = TextRenderer.Measure(label);
            var position = new Vector2(
                widget.Bounds.X + (widget.Bounds.Width - size.X) * 0.5f,
                widget.Bounds.Y + widget.Bounds.Height + 2);
            TextRenderer.Draw(batch, label, position, Theme.LabelColor * alpha);
        }

        /// <summary>The layout editor (null when the service was created without one).</summary>
        public TouchLayoutEditor Editor
        {
            get { return editor; }
        }

        /// <summary>
        /// Optional store used to persist the profiles. When it is set the layout
        /// editor offers a profile browser.
        /// </summary>
        public TouchControlProfileStore ProfileStore { get; set; }

        /// <summary>Saves the current layout under the given name.</summary>
        public void SaveProfile(string name)
        {
            if (ProfileStore == null) return;
            if (string.IsNullOrEmpty(name)) name = Profile.Name;
            Profile.Name = name;
            StoreLayoutIntoProfile();
            ProfileStore.Save(Profile);
        }

        /// <summary>Loads a stored profile. Returns false when it does not exist.</summary>
        public bool LoadProfileByName(string name)
        {
            if (ProfileStore == null) return false;
            var profile = ProfileStore.Load(name);
            if (profile == null) return false;
            profile.Name = name;
            LoadProfile(profile);
            return true;
        }

        /// <summary>Called when the graphics device is reset (e.g. after a rotation).</summary>
        public void OnDeviceReset()
        {
            TouchPrimitives.Clear();
            if (batch != null && !batch.IsDisposed) batch.Dispose();
            batch = new SpriteBatch(graphicsDevice);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (batch != null && !batch.IsDisposed) batch.Dispose();
            icons.Dispose();
        }
    }
}
