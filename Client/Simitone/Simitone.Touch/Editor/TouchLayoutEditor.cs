using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Touch.Input;
using Simitone.Touch.Widgets;

namespace Simitone.Touch.Editor
{
    /// <summary>
    /// The layout editor of the on-screen control system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the editor is open every control is outlined and can be dragged
    /// around, resized with the handle in the lower right corner, hidden and
    /// faded. A toolbar at the top of the screen gives access to the global
    /// settings, the profile management and the "done" button.
    /// </para>
    /// <para>
    /// The editor is drawn by the touch module itself, so it does not depend on
    /// the game's user interface and works on every screen.
    /// </para>
    /// </remarks>
    public class TouchLayoutEditor
    {
        private class EditorButton
        {
            public string Id;
            public string Label;
            public Rectangle Bounds;
            public Action OnClick;
            public bool Highlight;
            public bool Disabled;
        }

        private readonly TouchControlService service;
        private readonly List<EditorButton> buttons = new List<EditorButton>();

        private TouchWidget selected;
        private int dragContactId = -1;
        private TouchWidget dragWidget;
        private bool dragging;
        private bool resizing;
        private Vector2 dragStart;
        private Rectangle dragStartBounds;

        private bool showProfiles;
        private int profileSelection = -1;

        /// <summary>True while the editor is open.</summary>
        public bool Active { get; set; }

        /// <summary>The widget that is currently selected.</summary>
        public TouchWidget Selected
        {
            get { return selected; }
        }

        public TouchLayoutEditor(TouchControlService service)
        {
            this.service = service;
        }

        /// <summary>Handles the fingers while the editor is open.</summary>
        public void Update(IList<TouchContact> contacts, float deltaTime)
        {
            buttons.Clear();
            BuildToolbar();

            // --- buttons --------------------------------------------------------
            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                if (contact.State != TouchContactState.Pressed) continue;

                foreach (var button in buttons)
                {
                    if (button.Disabled) continue;
                    if (!button.Bounds.Contains(contact.Position)) continue;
                    if (button.OnClick != null) button.OnClick();
                    return;
                }
            }

            // --- dragging ------------------------------------------------------
            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];
                if (contact.State == TouchContactState.Pressed)
                {
                    if (dragContactId != -1) continue;

                    var widget = FindWidgetAt(contact.Position);
                    if (widget == null)
                    {
                        selected = null;
                        continue;
                    }

                    selected = widget;
                    dragContactId = contact.Id;
                    dragWidget = widget;
                    dragStart = contact.Position;
                    dragStartBounds = widget.Bounds;
                    resizing = IsOnResizeHandle(widget, contact.Position);
                    dragging = true;
                }
                else if (contact.State == TouchContactState.Moved && contact.Id == dragContactId && dragWidget != null)
                {
                    ApplyDrag(contact.Position);
                }
                else if (contact.State == TouchContactState.Released && contact.Id == dragContactId)
                {
                    dragContactId = -1;
                    dragWidget = null;
                    dragging = false;
                    resizing = false;
                    service.StoreLayoutIntoProfile();
                }
            }
        }

        private void ApplyDrag(Vector2 position)
        {
            if (dragWidget == null) return;
            var delta = position - dragStart;
            var bounds = dragStartBounds;

            if (resizing)
            {
                int size = (int)(bounds.Width + delta.X);
                int minSize = Math.Max(24, service.ScreenHeight / 12);
                size = Math.Max(minSize, Math.Min(size, service.ScreenHeight));
                bounds.Width = size;
                bounds.Height = size;
            }
            else
            {
                bounds.X += (int)delta.X;
                bounds.Y += (int)delta.Y;
            }

            if (service.Settings.SnapToGrid && service.Settings.GridSize > 1)
            {
                int cell = service.Settings.GridSize;
                bounds.X = (int)Math.Round(bounds.X / (float)cell) * cell;
                bounds.Y = (int)Math.Round(bounds.Y / (float)cell) * cell;
            }

            // Keep the control on the screen.
            bounds.X = Math.Max(0, Math.Min(bounds.X, Math.Max(0, service.ScreenWidth - bounds.Width)));
            bounds.Y = Math.Max(0, Math.Min(bounds.Y, Math.Max(0, service.ScreenHeight - bounds.Height)));

            dragWidget.Layout.ApplyBounds(bounds, service.ScreenWidth, service.ScreenHeight);
            dragWidget.Bounds = dragWidget.Layout.Resolve(service.ScreenWidth, service.ScreenHeight, service.Settings.Scale);
        }

        private TouchWidget FindWidgetAt(Vector2 position)
        {
            var widgets = service.Widgets;
            for (int i = widgets.Count - 1; i >= 0; i--)
            {
                var widget = widgets[i];
                if (!widget.Layout.Visible) continue;
                if (widget.Bounds.Contains(position)) return widget;
            }
            return null;
        }

        private bool IsOnResizeHandle(TouchWidget widget, Vector2 position)
        {
            int handle = HandleSize;
            var handleBounds = new Rectangle(
                widget.Bounds.Right - handle,
                widget.Bounds.Bottom - handle,
                handle,
                handle);
            return handleBounds.Contains(position);
        }

        private const int HandleSize = 44;

        #region toolbar

        private void BuildToolbar()
        {
            int height = Math.Max(40, service.ScreenHeight / 14);
            int width = Math.Max(56, height * 2);
            int padding = Math.Max(6, height / 5);
            int y = padding;

            int x = service.ScreenWidth / 2 - (5 * (width + padding) - padding) / 2;

            AddButton("done", "Done", new Rectangle(x, y, width, height), () =>
            {
                service.StoreLayoutIntoProfile();
                service.EditMode = false;
            });
            x += width + padding;

            AddButton("grid", "Grid", new Rectangle(x, y, width, height), () =>
            {
                service.Settings.SnapToGrid = !service.Settings.SnapToGrid;
            }, service.Settings.SnapToGrid);
            x += width + padding;

            AddButton("opacity_down", "Opacity -", new Rectangle(x, y, width, height), () =>
            {
                service.Settings.Opacity = Math.Max(0.1f, service.Settings.Opacity - 0.1f);
            });
            x += width + padding;

            AddButton("opacity_up", "Opacity +", new Rectangle(x, y, width, height), () =>
            {
                service.Settings.Opacity = Math.Min(1f, service.Settings.Opacity + 0.1f);
            });
            x += width + padding;

            AddButton("profiles", "Layouts", new Rectangle(x, y, width, height), () =>
            {
                showProfiles = !showProfiles;
            });
            x += width + padding;

            if (selected != null)
            {
                AddButton("hide", selected.Layout.Visible ? "Hide" : "Show", new Rectangle(x, y, width, height), () =>
                {
                    selected.Layout.Visible = !selected.Layout.Visible;
                });
                x += width + padding;

                AddButton("smaller", "Size -", new Rectangle(x, y, width, height), () =>
                {
                    selected.Layout.Width = Math.Max(0.04f, selected.Layout.Width - 0.015f);
                    selected.Layout.Height = Math.Max(0.04f, selected.Layout.Height - 0.015f);
                });
                x += width + padding;

                AddButton("bigger", "Size +", new Rectangle(x, y, width, height), () =>
                {
                    selected.Layout.Width = Math.Min(0.6f, selected.Layout.Width + 0.015f);
                    selected.Layout.Height = Math.Min(0.6f, selected.Layout.Height + 0.015f);
                });
            }

            BuildProfileButtons();
        }

        private void BuildProfileButtons()
        {
            if (!showProfiles || service.ProfileStore == null) return;

            int height = Math.Max(36, service.ScreenHeight / 16);
            int width = Math.Max(180, height * 5);
            int x = service.ScreenWidth / 2 - width / 2;
            int y = service.ScreenHeight / 2 - height * 3;

            var names = service.ProfileStore.ListProfiles();
            int index = 0;
            foreach (var name in names)
            {
                var captured = name;
                var bounds = new Rectangle(x, y + index * (height + 4), width, height);
                AddButton("profile_" + name, name, bounds, () =>
                {
                    var profile = service.ProfileStore.Load(captured);
                    if (profile != null)
                    {
                        profile.Name = captured;
                        service.LoadProfile(profile);
                    }
                }, index == profileSelection);
                index++;
            }

            // "Save as" and "delete" for the currently selected profile.
            var actionY = y + index * (height + 4);
            var saveBounds = new Rectangle(x, actionY, width / 2 - 4, height);
            AddButton("profile_save", "Save", saveBounds, () =>
            {
                service.StoreLayoutIntoProfile();
                service.ProfileStore.Save(service.Profile);
            });

            var deleteBounds = new Rectangle(x + width / 2 + 4, actionY, width / 2 - 4, height);
            AddButton("profile_delete", "Delete", deleteBounds, () =>
            {
                if (profileSelection >= 0 && profileSelection < names.Count)
                {
                    service.ProfileStore.Delete(names[profileSelection]);
                    profileSelection = -1;
                }
            });
        }

        private void AddButton(string id, string label, Rectangle bounds, Action onClick, bool highlight = false)
        {
            buttons.Add(new EditorButton
            {
                Id = id,
                Label = label,
                Bounds = bounds,
                OnClick = onClick,
                Highlight = highlight
            });
        }

        #endregion

        /// <summary>Draws the editor.</summary>
        public void Draw(SpriteBatch batch, TouchWidgetContext context)
        {
            var theme = context.Theme;
            float alpha = context.GlobalOpacity;

            // --- outlines -------------------------------------------------------
            foreach (var widget in service.Widgets)
            {
                if (!widget.Layout.Visible) continue;

                var bounds = widget.Bounds;
                var outline = widget == selected ? theme.EditOutlineSelected : theme.EditOutline;
                DrawFrame(batch, bounds, outline * alpha, 2);

                if (widget == selected)
                {
                    var handle = new Rectangle(
                        bounds.Right - HandleSize,
                        bounds.Bottom - HandleSize,
                        HandleSize,
                        HandleSize);
                    var circle = TouchPrimitives.GetCircle(batch.GraphicsDevice);
                    batch.Draw(circle, handle, theme.EditHandle * alpha);
                }
            }

            // --- toolbar --------------------------------------------------------
            foreach (var button in buttons)
            {
                var circle = TouchPrimitives.GetCircle(batch.GraphicsDevice);
                var color = button.Highlight
                    ? theme.ButtonBackgroundPressed
                    : theme.EditorPanel;
                batch.Draw(circle, button.Bounds, color * alpha);

                var frame = button.Highlight ? theme.EditOutlineSelected : theme.EditorText;
                DrawFrame(batch, button.Bounds, frame * (button.Highlight ? alpha : alpha * 0.6f), 2);

                if (context.TextRenderer != null && !string.IsNullOrEmpty(button.Label))
                {
                    var size = context.TextRenderer.Measure(button.Label);
                    var position = new Vector2(
                        button.Bounds.X + (button.Bounds.Width - size.X) * 0.5f,
                        button.Bounds.Y + (button.Bounds.Height - size.Y) * 0.5f);
                    context.TextRenderer.Draw(batch, button.Label, position, theme.EditorText * alpha);
                }
            }

            // --- profile list ---------------------------------------------------
            if (showProfiles && service.ProfileStore != null)
            {
                DrawProfileList(batch, context);
            }
        }

        private void DrawProfileList(SpriteBatch batch, TouchWidgetContext context)
        {
            var theme = context.Theme;
            float alpha = context.GlobalOpacity;

            int height = Math.Max(36, service.ScreenHeight / 16);
            int width = Math.Max(180, height * 5);
            int x = service.ScreenWidth / 2 - width / 2;
            int y = service.ScreenHeight / 2 - height * 3;

            var panel = new Rectangle(x - 10, y - 10, width + 20, height * 9 + 20);
            var circle = TouchPrimitives.GetCircle(batch.GraphicsDevice);
            batch.Draw(circle, panel, theme.EditorPanel * alpha);
            DrawFrame(batch, panel, theme.EditOutline * alpha, 2);

            foreach (var button in buttons)
            {
                if (button.Id == null || !button.Id.StartsWith("profile_")) continue;
                if (context.TextRenderer == null) break;
                var size = context.TextRenderer.Measure(button.Label);
                var position = new Vector2(
                    button.Bounds.X + (button.Bounds.Width - size.X) * 0.5f,
                    button.Bounds.Y + (button.Bounds.Height - size.Y) * 0.5f);
                context.TextRenderer.Draw(batch, button.Label, position, theme.EditorText * alpha);
            }
        }

        private static void DrawFrame(SpriteBatch batch, Rectangle bounds, Color color, int thickness)
        {
            var circle = TouchPrimitives.GetCircle(batch.GraphicsDevice);
            // The frame is drawn as four thin rectangles.
            batch.Draw(circle, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
            batch.Draw(circle, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
            batch.Draw(circle, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
            batch.Draw(circle, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
        }
    }
}
