using FSO.Common.Rendering.Framework;
using FSO.Common.Rendering.Framework.Model;
using Microsoft.Xna.Framework.Graphics;
using Simitone.Touch;

namespace Simitone.Client.UI.Touch
{
    /// <summary>
    /// Draws the on-screen controls on top of the game.
    /// </summary>
    /// <remarks>
    /// The layer is appended to the end of <c>GameScreen.Layers</c>, so it is
    /// drawn after the user interface and therefore appears on top of it. It
    /// owns its own sprite batch, the batches of the engine are closed when a
    /// layer is drawn.
    /// </remarks>
    public class TouchOverlayLayer : IGraphicsLayer
    {
        private readonly TouchControlService service;

        public TouchOverlayLayer(TouchControlService service)
        {
            this.service = service;
        }

        public void Initialize(GraphicsDevice device)
        {
        }

        public void Update(UpdateState state)
        {
        }

        public void PreDraw(GraphicsDevice device)
        {
        }

        public void Draw(GraphicsDevice device)
        {
            if (service == null) return;
            service.Draw();
        }
    }
}
