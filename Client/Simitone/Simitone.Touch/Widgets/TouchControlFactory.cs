using System;
using System.Collections.Generic;

namespace Simitone.Touch.Widgets
{
    /// <summary>
    /// Creates the default set of on-screen controls.
    /// </summary>
    /// <remarks>
    /// The factory is the single place that knows what the default layout looks
    /// like. Everything is expressed in relative units so that the same layout
    /// works on a phone and on a tablet.
    /// </remarks>
    public static class TouchControlFactory
    {
        // --- widget ids ---------------------------------------------------------
        public const string CameraStick = "camera_stick";
        public const string ZoomIn = "zoom_in";
        public const string ZoomOut = "zoom_out";
        public const string RotateLeft = "rotate_left";
        public const string RotateRight = "rotate_right";
        public const string Pause = "pause";
        public const string Speed1 = "speed_1";
        public const string Speed2 = "speed_2";
        public const string Speed3 = "speed_3";
        public const string Menu = "menu";
        public const string Options = "options";
        public const string Edit = "edit";
        public const string ModeLive = "mode_live";
        public const string ModeBuild = "mode_build";
        public const string ModeBuy = "mode_buy";
        public const string Inventory = "inventory";
        public const string Family = "family";
        public const string Skills = "skills";
        public const string Cancel = "cancel";
        public const string FreeCamera = "free_camera";
        public const string FrameAdvance = "frame_advance";

        /// <summary>Builds the default widget list.</summary>
        public static List<TouchWidget> CreateWidgets(TouchControlProfile profile)
        {
            var widgets = new List<TouchWidget>();

            // --- camera stick ----------------------------------------------------
            var stick = new TouchJoystick(CameraStick) { Floating = true };
            stick.Layout = Layout(TouchAnchor.BottomLeft, 0.025f, 0.075f, 0.24f, 0.24f);
            widgets.Add(stick);

            // --- camera buttons -------------------------------------------------
            var zoomIn = new TouchButton(ZoomIn, TouchAction.ZoomIn, "zoom_in", TouchButton.ButtonMode.Hold);
            zoomIn.Layout = Layout(TouchAnchor.BottomRight, 0.022f, 0.075f, 0.115f, 0.115f);
            widgets.Add(zoomIn);

            var zoomOut = new TouchButton(ZoomOut, TouchAction.ZoomOut, "zoom_out", TouchButton.ButtonMode.Hold);
            zoomOut.Layout = Layout(TouchAnchor.BottomRight, 0.022f, 0.205f, 0.115f, 0.115f);
            widgets.Add(zoomOut);

            var rotateLeft = new TouchButton(RotateLeft, TouchAction.RotateLeft, "rotate_left", TouchButton.ButtonMode.Hold);
            rotateLeft.Layout = Layout(TouchAnchor.BottomRight, 0.145f, 0.075f, 0.115f, 0.115f);
            widgets.Add(rotateLeft);

            var rotateRight = new TouchButton(RotateRight, TouchAction.RotateRight, "rotate_right", TouchButton.ButtonMode.Hold);
            rotateRight.Layout = Layout(TouchAnchor.BottomRight, 0.145f, 0.205f, 0.115f, 0.115f);
            widgets.Add(rotateRight);

            // --- speed ----------------------------------------------------------
            var pause = new TouchButton(Pause, TouchAction.Pause, "pause", TouchButton.ButtonMode.Tap);
            pause.Layout = Layout(TouchAnchor.TopRight, 0.022f, 0.035f, 0.10f, 0.10f);
            widgets.Add(pause);

            var speed1 = new TouchButton(Speed1, TouchAction.Speed1, "speed_1", TouchButton.ButtonMode.Tap);
            speed1.Layout = Layout(TouchAnchor.TopRight, 0.135f, 0.035f, 0.10f, 0.10f);
            widgets.Add(speed1);

            var speed2 = new TouchButton(Speed2, TouchAction.Speed2, "speed_2", TouchButton.ButtonMode.Tap);
            speed2.Layout = Layout(TouchAnchor.TopRight, 0.248f, 0.035f, 0.10f, 0.10f);
            widgets.Add(speed2);

            var speed3 = new TouchButton(Speed3, TouchAction.Speed3, "speed_3", TouchButton.ButtonMode.Tap);
            speed3.Layout = Layout(TouchAnchor.TopRight, 0.361f, 0.035f, 0.10f, 0.10f);
            widgets.Add(speed3);

            // --- system ----------------------------------------------------------
            var menu = new TouchButton(Menu, TouchAction.Menu, "menu", TouchButton.ButtonMode.Tap);
            menu.Layout = Layout(TouchAnchor.TopLeft, 0.022f, 0.035f, 0.10f, 0.10f);
            widgets.Add(menu);

            var options = new TouchButton(Options, TouchAction.OpenOptions, "options", TouchButton.ButtonMode.Tap);
            options.Layout = Layout(TouchAnchor.TopLeft, 0.022f, 0.145f, 0.10f, 0.10f);
            widgets.Add(options);

            var edit = new TouchButton(Edit, TouchAction.EditControls, "edit", TouchButton.ButtonMode.Toggle);
            edit.Layout = Layout(TouchAnchor.TopLeft, 0.022f, 0.255f, 0.10f, 0.10f);
            widgets.Add(edit);

            // --- optional (hidden by default) ------------------------------------
            var modeLive = new TouchButton(ModeLive, TouchAction.ModeLive, "mode_live", TouchButton.ButtonMode.Tap);
            modeLive.Layout = Layout(TouchAnchor.TopLeft, 0.135f, 0.035f, 0.10f, 0.10f);
            modeLive.Layout.Visible = false;
            widgets.Add(modeLive);

            var modeBuild = new TouchButton(ModeBuild, TouchAction.ModeBuild, "mode_build", TouchButton.ButtonMode.Tap);
            modeBuild.Layout = Layout(TouchAnchor.TopLeft, 0.248f, 0.035f, 0.10f, 0.10f);
            modeBuild.Layout.Visible = false;
            widgets.Add(modeBuild);

            var modeBuy = new TouchButton(ModeBuy, TouchAction.ModeBuy, "mode_buy", TouchButton.ButtonMode.Tap);
            modeBuy.Layout = Layout(TouchAnchor.TopLeft, 0.361f, 0.035f, 0.10f, 0.10f);
            modeBuy.Layout.Visible = false;
            widgets.Add(modeBuy);

            var inventory = new TouchButton(Inventory, TouchAction.OpenInventory, "inventory", TouchButton.ButtonMode.Tap);
            inventory.Layout = Layout(TouchAnchor.BottomRight, 0.268f, 0.075f, 0.10f, 0.10f);
            inventory.Layout.Visible = false;
            widgets.Add(inventory);

            var family = new TouchButton(Family, TouchAction.OpenFamily, "family", TouchButton.ButtonMode.Tap);
            family.Layout = Layout(TouchAnchor.BottomRight, 0.268f, 0.185f, 0.10f, 0.10f);
            family.Layout.Visible = false;
            widgets.Add(family);

            var skills = new TouchButton(Skills, TouchAction.OpenSkills, "skills", TouchButton.ButtonMode.Tap);
            skills.Layout = Layout(TouchAnchor.BottomRight, 0.268f, 0.295f, 0.10f, 0.10f);
            skills.Layout.Visible = false;
            widgets.Add(skills);

            var cancel = new TouchButton(Cancel, TouchAction.Cancel, "cancel", TouchButton.ButtonMode.Tap);
            cancel.Layout = Layout(TouchAnchor.BottomRight, 0.022f, 0.335f, 0.10f, 0.10f);
            cancel.Layout.Visible = false;
            widgets.Add(cancel);

            var freeCamera = new TouchButton(FreeCamera, TouchAction.FreeCamera, "free_camera", TouchButton.ButtonMode.Toggle);
            freeCamera.Layout = Layout(TouchAnchor.TopRight, 0.474f, 0.035f, 0.10f, 0.10f);
            freeCamera.Layout.Visible = false;
            widgets.Add(freeCamera);

            var frameAdvance = new TouchButton(FrameAdvance, TouchAction.FrameAdvance, "frame_advance", TouchButton.ButtonMode.Tap);
            frameAdvance.Layout = Layout(TouchAnchor.TopRight, 0.587f, 0.035f, 0.10f, 0.10f);
            frameAdvance.Layout.Visible = false;
            widgets.Add(frameAdvance);

            return widgets;
        }

        /// <summary>Creates a profile that contains the default layout.</summary>
        public static TouchControlProfile CreateDefaultProfile()
        {
            var profile = new TouchControlProfile
            {
                Name = "Default",
                Description = "The stock Simitone touch layout.",
                Settings = new TouchControlSettings(),
                IsBuiltIn = true
            };

            foreach (var widget in CreateWidgets(profile))
            {
                profile.Widgets[widget.Id] = widget.Layout.Clone();
                profile.Order.Add(widget.Id);
            }

            return profile;
        }

        private static TouchWidgetLayout Layout(TouchAnchor anchor, float x, float y, float width, float height)
        {
            return new TouchWidgetLayout
            {
                Anchor = anchor,
                OffsetX = x,
                OffsetY = y,
                Width = width,
                Height = height,
                Opacity = 1f,
                Visible = true,
                Scale = 1f
            };
        }
    }
}
