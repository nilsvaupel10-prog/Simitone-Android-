using System;
using System.Collections.Generic;
using FSO.Common.Rendering.Framework;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Simitone.Touch;

namespace Simitone.Client.UI.Touch
{
    /// <summary>
    /// The layer that reads the fingers of the on-screen controls and feeds them
    /// into the regular input pipeline of the engine.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The layer is inserted at the front of <c>GameScreen.Layers</c> so that it
    /// runs before every other layer. It
    /// </para>
    /// <list type="number">
    /// <item>removes the fingers that were used by the overlay from the mouse
    /// states, so that the game does not see them,</item>
    /// <item>replaces the (empty) keyboard state with the keys that the overlay
    /// wants to be pressed.</item>
    /// </list>
    /// <para>
    /// Nothing in the engine has to be changed for this: the camera, the zoom,
    /// the rotation and the game speed are all controlled by keys.
    /// </para>
    /// </remarks>
    public class TouchInputLayer : IGraphicsLayer
    {
        private readonly TouchControlService service;
        private readonly Func<Vector2> screenSizeProvider;

        public TouchInputLayer(TouchControlService service, Func<Vector2> screenSizeProvider)
        {
            this.service = service;
            this.screenSizeProvider = screenSizeProvider;
        }

        public void Initialize(GraphicsDevice device)
        {
        }

        public void Update(UpdateState state)
        {
            if (service == null || state == null) return;

            if (screenSizeProvider != null)
            {
                var size = screenSizeProvider();
                service.UpdateLayout((int)size.X, (int)size.Y);
            }

            float delta = 1f / 60f;
            if (state.Time != null)
            {
                delta = (float)state.Time.ElapsedGameTime.TotalSeconds;
                if (delta <= 0f) delta = 1f / 60f;
            }

            service.Update(delta);

            // The fingers that belong to the overlay must not reach the game.
            var consumed = service.ConsumedTouchIds;
            if (consumed != null && consumed.Count > 0 && state.MouseStates != null)
            {
                for (int i = state.MouseStates.Count - 1; i >= 0; i--)
                {
                    if (consumed.Contains(state.MouseStates[i].ID))
                    {
                        state.MouseStates.RemoveAt(i);
                    }
                }
            }

            // The virtual keys take the place of the (empty) keyboard.
            var keys = service.GetVirtualKeys();
            state.KeyboardState = new KeyboardState(keys);

            if (keys.Length > 0)
            {
                var previous = state.PreviousKeyboardState;
                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    bool isNew = previous == null || previous.IsKeyUp(key);
                    if (isNew && !state.NewKeys.Contains(key))
                    {
                        state.NewKeys.Add(key);
                    }
                }
            }
        }

        public void PreDraw(GraphicsDevice device)
        {
        }

        public void Draw(GraphicsDevice device)
        {
        }
    }
}
