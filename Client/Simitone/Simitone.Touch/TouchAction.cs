using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Simitone.Touch
{
    /// <summary>
    /// Every distinct thing the on-screen controls can do.
    /// </summary>
    /// <remarks>
    /// The touch system never talks to the engine directly. It reports actions and
    /// synthesised keyboard state, and the bridge (Simitone.Client) decides what an
    /// action means. That keeps the module free of engine dependencies and makes it
    /// trivial to re-target at another game.
    /// </remarks>
    public enum TouchAction
    {
        None = 0,

        // --- camera -------------------------------------------------------------
        /// <summary>Pan the camera "up" (away from the viewer).</summary>
        CameraForward,
        /// <summary>Pan the camera "down" (towards the viewer).</summary>
        CameraBack,
        /// <summary>Pan the camera to the left.</summary>
        CameraLeft,
        /// <summary>Pan the camera to the right.</summary>
        CameraRight,
        /// <summary>Increase the camera zoom (i.e. zoom in).</summary>
        ZoomIn,
        /// <summary>Decrease the camera zoom (i.e. zoom out).</summary>
        ZoomOut,
        /// <summary>Rotate the camera counter clockwise.</summary>
        RotateLeft,
        /// <summary>Rotate the camera clockwise.</summary>
        RotateRight,
        /// <summary>Toggle the free-look / first person camera.</summary>
        FreeCamera,

        // --- game flow ----------------------------------------------------------
        /// <summary>Pause the simulation.</summary>
        Pause,
        /// <summary>Resume the simulation.</summary>
        Resume,
        /// <summary>Set the simulation speed to 1x.</summary>
        Speed1,
        /// <summary>Set the simulation speed to 3x.</summary>
        Speed2,
        /// <summary>Set the simulation speed to 10x.</summary>
        Speed3,
        /// <summary>Advance a single frame while paused.</summary>
        FrameAdvance,

        // --- modes --------------------------------------------------------------
        /// <summary>Switch to live mode.</summary>
        ModeLive,
        /// <summary>Switch to build mode.</summary>
        ModeBuild,
        /// <summary>Switch to buy mode.</summary>
        ModeBuy,
        /// <summary>Open the in-game options.</summary>
        OpenOptions,
        /// <summary>Open the inventory of the selected sim.</summary>
        OpenInventory,
        /// <summary>Open the sims / family panel.</summary>
        OpenFamily,
        /// <summary>Open the skills and wants panel.</summary>
        OpenSkills,

        // --- misc ---------------------------------------------------------------
        /// <summary>Cancel the current interaction / close the top-most dialog.</summary>
        Cancel,
        /// <summary>Open the touch control layout editor.</summary>
        EditControls,
        /// <summary>Bring up the in-game (or system) menu.</summary>
        Menu
    }

    /// <summary>
    /// Static description of an action: how it is presented to the user and which
    /// keyboard keys it is translated into.
    /// </summary>
    public class TouchActionDescriptor
    {
        public TouchAction Action;
        public string DisplayName;
        public string IconName;
        public TouchActionGroup Group;
        /// <summary>Keys that are held down while the control is active.</summary>
        public Keys[] HeldKeys = new Keys[0];
        /// <summary>Keys that are "tapped" (pressed for exactly one frame) on activation.</summary>
        public Keys[] TappedKeys = new Keys[0];
        /// <summary>True when the control represents a two state (on/off) mode.</summary>
        public bool IsToggle;
        /// <summary>True when the action should be triggered repeatedly while held.</summary>
        public bool Repeatable;

        public TouchActionDescriptor()
        {
        }

        public TouchActionDescriptor(TouchAction action, string displayName, string iconName, TouchActionGroup group)
        {
            Action = action;
            DisplayName = displayName;
            IconName = iconName;
            Group = group;
        }
    }

    /// <summary>
    /// Grouping used by the layout editor so that related controls can be shown
    /// together.
    /// </summary>
    public enum TouchActionGroup
    {
        Camera,
        Speed,
        Mode,
        Interface,
        System
    }

    /// <summary>
    /// The catalogue of all known actions.
    /// </summary>
    public static class TouchActionCatalog
    {
        private static readonly Dictionary<TouchAction, TouchActionDescriptor> Descriptors =
            new Dictionary<TouchAction, TouchActionDescriptor>();

        static TouchActionCatalog()
        {
            Add(new TouchActionDescriptor(TouchAction.CameraForward, "Move forward", "camera_forward", TouchActionGroup.Camera)
            {
                HeldKeys = new[] { Keys.W, Keys.Up },
                Repeatable = true
            });
            Add(new TouchActionDescriptor(TouchAction.CameraBack, "Move back", "camera_back", TouchActionGroup.Camera)
            {
                HeldKeys = new[] { Keys.S, Keys.Down },
                Repeatable = true
            });
            Add(new TouchActionDescriptor(TouchAction.CameraLeft, "Move left", "camera_left", TouchActionGroup.Camera)
            {
                HeldKeys = new[] { Keys.A, Keys.Left },
                Repeatable = true
            });
            Add(new TouchActionDescriptor(TouchAction.CameraRight, "Move right", "camera_right", TouchActionGroup.Camera)
            {
                HeldKeys = new[] { Keys.D, Keys.Right },
                Repeatable = true
            });
            Add(new TouchActionDescriptor(TouchAction.ZoomIn, "Zoom in", "zoom_in", TouchActionGroup.Camera)
            {
                HeldKeys = new[] { Keys.OemPlus },
                Repeatable = true
            });
            Add(new TouchActionDescriptor(TouchAction.ZoomOut, "Zoom out", "zoom_out", TouchActionGroup.Camera)
            {
                HeldKeys = new[] { Keys.OemMinus },
                Repeatable = true
            });
            Add(new TouchActionDescriptor(TouchAction.RotateLeft, "Rotate left", "rotate_left", TouchActionGroup.Camera)
            {
                HeldKeys = new[] { Keys.OemComma },
                Repeatable = true
            });
            Add(new TouchActionDescriptor(TouchAction.RotateRight, "Rotate right", "rotate_right", TouchActionGroup.Camera)
            {
                HeldKeys = new[] { Keys.OemPeriod },
                Repeatable = true
            });
            Add(new TouchActionDescriptor(TouchAction.FreeCamera, "Free camera", "free_camera", TouchActionGroup.Camera)
            {
                IsToggle = true
            });

            Add(new TouchActionDescriptor(TouchAction.Pause, "Pause", "pause", TouchActionGroup.Speed)
            {
                TappedKeys = new[] { Keys.P }
            });
            Add(new TouchActionDescriptor(TouchAction.Resume, "Resume", "play", TouchActionGroup.Speed)
            {
                TappedKeys = new[] { Keys.P }
            });
            Add(new TouchActionDescriptor(TouchAction.Speed1, "Speed 1x", "speed_1", TouchActionGroup.Speed)
            {
                TappedKeys = new[] { Keys.D1 }
            });
            Add(new TouchActionDescriptor(TouchAction.Speed2, "Speed 3x", "speed_2", TouchActionGroup.Speed)
            {
                TappedKeys = new[] { Keys.D2 }
            });
            Add(new TouchActionDescriptor(TouchAction.Speed3, "Speed 10x", "speed_3", TouchActionGroup.Speed)
            {
                TappedKeys = new[] { Keys.D3 }
            });
            Add(new TouchActionDescriptor(TouchAction.FrameAdvance, "Next frame", "frame_advance", TouchActionGroup.Speed)
            {
                TappedKeys = new[] { Keys.D0 }
            });

            Add(new TouchActionDescriptor(TouchAction.ModeLive, "Live mode", "mode_live", TouchActionGroup.Mode));
            Add(new TouchActionDescriptor(TouchAction.ModeBuild, "Build mode", "mode_build", TouchActionGroup.Mode));
            Add(new TouchActionDescriptor(TouchAction.ModeBuy, "Buy mode", "mode_buy", TouchActionGroup.Mode));

            Add(new TouchActionDescriptor(TouchAction.OpenOptions, "Options", "options", TouchActionGroup.Interface));
            Add(new TouchActionDescriptor(TouchAction.OpenInventory, "Inventory", "inventory", TouchActionGroup.Interface));
            Add(new TouchActionDescriptor(TouchAction.OpenFamily, "Family", "family", TouchActionGroup.Interface));
            Add(new TouchActionDescriptor(TouchAction.OpenSkills, "Skills", "skills", TouchActionGroup.Interface));
            Add(new TouchActionDescriptor(TouchAction.Cancel, "Cancel", "cancel", TouchActionGroup.Interface)
            {
                TappedKeys = new[] { Keys.Escape }
            });

            Add(new TouchActionDescriptor(TouchAction.EditControls, "Edit controls", "edit", TouchActionGroup.System)
            {
                IsToggle = true
            });
            Add(new TouchActionDescriptor(TouchAction.Menu, "Menu", "menu", TouchActionGroup.System)
            {
                IsToggle = true
            });
        }

        private static void Add(TouchActionDescriptor descriptor)
        {
            Descriptors[descriptor.Action] = descriptor;
        }

        /// <summary>All known descriptors, in declaration order.</summary>
        public static IEnumerable<TouchActionDescriptor> All
        {
            get { return Descriptors.Values; }
        }

        /// <summary>Returns the descriptor for an action (never null).</summary>
        public static TouchActionDescriptor Get(TouchAction action)
        {
            TouchActionDescriptor descriptor;
            if (Descriptors.TryGetValue(action, out descriptor)) return descriptor;
            return new TouchActionDescriptor(action, action.ToString(), "unknown", TouchActionGroup.System);
        }

        /// <summary>Looks up an action by its stable identifier (used by the profile files).</summary>
        public static TouchAction? FromId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var pair in Descriptors)
            {
                if (string.Equals(pair.Key.ToString(), id, StringComparison.OrdinalIgnoreCase)) return pair.Key;
            }
            return null;
        }
    }
}
