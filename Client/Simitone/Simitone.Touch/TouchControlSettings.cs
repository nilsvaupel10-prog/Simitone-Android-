using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Simitone.Touch
{
    /// <summary>
    /// Global, user editable settings of the on-screen control system.
    /// </summary>
    public class TouchControlSettings
    {
        /// <summary>Master opacity of the whole overlay (0 - 1).</summary>
        public float Opacity { get; set; } = 0.85f;

        /// <summary>Master scale of the overlay (0.5 - 2).</summary>
        public float Scale { get; set; } = 1f;

        /// <summary>Whether haptic feedback is used when a control is pressed.</summary>
        public bool Haptics { get; set; } = true;

        /// <summary>Strength of the haptic feedback (0 - 1).</summary>
        public float HapticStrength { get; set; } = 0.5f;

        /// <summary>Enables the two finger pinch to zoom gesture.</summary>
        public bool PinchToZoom { get; set; } = true;

        /// <summary>Enables the two finger rotate gesture.</summary>
        public bool TwoFingerRotate { get; set; } = true;

        /// <summary>Enables one finger drag to pan the camera.</summary>
        public bool DragToPan { get; set; } = true;

        /// <summary>Dead zone of the virtual stick, as a fraction of the stick radius.</summary>
        public float StickDeadZone { get; set; } = 0.18f;

        /// <summary>When enabled the layout editor snaps controls to a grid.</summary>
        public bool SnapToGrid { get; set; } = true;

        /// <summary>Grid size in pixels used by the layout editor.</summary>
        public int GridSize { get; set; } = 16;

        /// <summary>Shows a short label underneath every control.</summary>
        public bool ShowLabels { get; set; } = true;

        /// <summary>Name of the profile that is currently active.</summary>
        public string ActiveProfile { get; set; } = "Default";

        public TouchControlSettings Clone()
        {
            return (TouchControlSettings)MemberwiseClone();
        }

        public void Sanitize()
        {
            Opacity = Clamp(Opacity, 0f, 1f);
            Scale = Clamp(Scale, 0.5f, 2f);
            HapticStrength = Clamp(HapticStrength, 0f, 1f);
            StickDeadZone = Clamp(StickDeadZone, 0f, 0.9f);
            if (GridSize < 4) GridSize = 4;
            if (GridSize > 128) GridSize = 128;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    /// <summary>
    /// A complete, serialisable description of an on-screen control layout.
    /// </summary>
    public class TouchControlProfile
    {
        /// <summary>Name of the profile ("Default", "One handed", "Tablet", ...).</summary>
        public string Name { get; set; } = "Default";

        /// <summary>Free form description shown in the editor.</summary>
        public string Description { get; set; } = "";

        /// <summary>Settings that belong to this profile.</summary>
        public TouchControlSettings Settings { get; set; } = new TouchControlSettings();

        /// <summary>Placement of every widget, keyed by widget id.</summary>
        public Dictionary<string, TouchWidgetLayout> Widgets { get; set; } = new Dictionary<string, TouchWidgetLayout>();

        /// <summary>Order the widgets are drawn in (and hit tested in). Later = on top.</summary>
        public List<string> Order { get; set; } = new List<string>();

        [JsonIgnore]
        public bool IsBuiltIn { get; set; }

        public TouchControlProfile Clone()
        {
            var clone = new TouchControlProfile
            {
                Name = Name,
                Description = Description,
                Settings = Settings == null ? new TouchControlSettings() : Settings.Clone(),
                Widgets = new Dictionary<string, TouchWidgetLayout>(),
                Order = new List<string>(Order),
                IsBuiltIn = IsBuiltIn
            };
            foreach (var pair in Widgets)
            {
                clone.Widgets[pair.Key] = pair.Value == null ? null : pair.Value.Clone();
            }
            return clone;
        }

        /// <summary>Returns the layout of a widget, creating it when it does not exist yet.</summary>
        public TouchWidgetLayout GetOrCreate(string id, Func<TouchWidgetLayout> factory)
        {
            TouchWidgetLayout layout;
            if (!Widgets.TryGetValue(id, out layout) || layout == null)
            {
                layout = factory();
                layout.Id = id;
                Widgets[id] = layout;
            }
            return layout;
        }
    }
}
