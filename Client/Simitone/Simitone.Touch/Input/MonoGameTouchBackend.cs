using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Simitone.Touch.Input
{
    /// <summary>
    /// Default backend: reads the fingers from MonoGame's <see cref="TouchPanel"/>.
    /// </summary>
    /// <remarks>
    /// MonoGame hands out a <see cref="TouchCollection"/> where every entry already
    /// carries the previous position and the state of the finger, so the translation
    /// into <see cref="TouchContact"/> is a straight forward mapping.
    /// </remarks>
    public class MonoGameTouchBackend : ITouchInputBackend
    {
        private readonly List<TouchContact> contacts = new List<TouchContact>();
        private readonly List<TouchContact> released = new List<TouchContact>();

        /// <summary>
        /// When true, touches that start on top of an on-screen control are not
        /// forwarded to the game (they are "consumed" by the overlay).
        /// </summary>
        public bool ConsumeControlTouches { get; set; }

        /// <summary>Ids of the touches that were consumed by the overlay.</summary>
        public HashSet<int> ConsumedTouchIds { get; } = new HashSet<int>();

        public IList<TouchContact> GetContacts()
        {
            return contacts;
        }

        /// <summary>The touches that ended during this frame.</summary>
        public IList<TouchContact> ReleasedContacts
        {
            get { return released; }
        }

        public void BeginFrame()
        {
            contacts.Clear();
            released.Clear();
            ConsumedTouchIds.Clear();

            TouchCollection touches;
            try
            {
                touches = TouchPanel.GetState();
            }
            catch (System.Exception)
            {
                // TouchPanel throws when there is no primary window (e.g. during
                // unit tests). Treat that as "no fingers".
                return;
            }

            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                TouchContactState state;
                switch (touch.State)
                {
                    case TouchLocationState.Pressed:
                        state = TouchContactState.Pressed;
                        break;
                    case TouchLocationState.Moved:
                        state = TouchContactState.Moved;
                        break;
                    default:
                        state = TouchContactState.Released;
                        break;
                }

                var contact = new TouchContact(touch.Id, touch.Position, state);
                contacts.Add(contact);
                if (state == TouchContactState.Released) released.Add(contact);
            }
        }

        public void EndFrame()
        {
        }

        /// <summary>
        /// Marks a touch as consumed by the overlay so that the game does not see it.
        /// </summary>
        public void Consume(int touchId)
        {
            ConsumedTouchIds.Add(touchId);
        }
    }
}
