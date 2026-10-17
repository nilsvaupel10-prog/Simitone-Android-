using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Simitone.Touch.Input
{
    /// <summary>
    /// The state of a single finger.
    /// </summary>
    public enum TouchContactState
    {
        Pressed,
        Moved,
        Released
    }

    /// <summary>
    /// A single finger reported by the platform.
    /// </summary>
    public struct TouchContact
    {
        public int Id;
        public Vector2 Position;
        public TouchContactState State;

        public TouchContact(int id, Vector2 position, TouchContactState state)
        {
            Id = id;
            Position = position;
            State = state;
        }
    }

    /// <summary>
    /// Abstraction over the platform touch input.
    /// </summary>
    /// <remarks>
    /// Keeping this behind an interface means the whole control system can be
    /// driven by a mouse (for testing on the desktop) or by a replay, and it keeps
    /// MonoGame specific code in a single place.
    /// </remarks>
    public interface ITouchInputBackend
    {
        /// <summary>All fingers that are currently on the screen.</summary>
        IList<TouchContact> GetContacts();

        /// <summary>Called once per frame before the widgets are updated.</summary>
        void BeginFrame();

        /// <summary>Called once per frame after the widgets have been updated.</summary>
        void EndFrame();
    }

    /// <summary>
    /// Haptic feedback abstraction.
    /// </summary>
    public interface IHapticService
    {
        /// <summary>Short "tick" used when a control is pressed.</summary>
        void LightTick(float strength);

        /// <summary>Longer buzz used for confirmations.</summary>
        void Confirm(float strength);
    }

    /// <summary>
    /// Haptic service that does nothing - used on platforms without a vibrator.
    /// </summary>
    public class NullHapticService : IHapticService
    {
        public static readonly NullHapticService Instance = new NullHapticService();

        public void LightTick(float strength)
        {
        }

        public void Confirm(float strength)
        {
        }
    }
}
