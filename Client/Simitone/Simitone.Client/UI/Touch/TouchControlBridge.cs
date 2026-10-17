using System;
using System.Collections.Generic;
using System.IO;
using FSO.Client;
using FSO.Common;
using FSO.Common.Rendering.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSDFData;
using Simitone.Client.UI.Panels;
using Simitone.Client.UI.Screens;
using Simitone.Touch;
using Simitone.Touch.Input;

namespace Simitone.Client.UI.Touch
{
    /// <summary>
    /// Connects the on-screen control module with the game.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the only place where the touch module and the engine meet. The
    /// module itself does not know anything about The Sims: it produces key
    /// presses and actions, the bridge decides what they mean.
    /// </para>
    /// <para>
    /// The bridge adds two layers to the screen:
    /// </para>
    /// <list type="bullet">
    /// <item>an input layer at the very front that reads the fingers and feeds
    /// the synthesised keys into the input state,</item>
    /// <item>an overlay layer at the very end that draws the controls.</item>
    /// </list>
    /// <para>
    /// The engine itself is not modified in any way.
    /// </para>
    /// </remarks>
    public static class TouchControlBridge
    {
        private static TouchControlService service;
        private static TouchInputLayer inputLayer;
        private static TouchOverlayLayer overlayLayer;

        /// <summary>
        /// Raised for actions that the engine does not know about, so that the
        /// host (for example the Android app) can handle them.
        /// </summary>
        public static event Action<TouchAction> HostActionRequested;

        /// <summary>The control system (null when the touch mode is not active).</summary>
        public static TouchControlService Service
        {
            get { return service; }
        }

        /// <summary>True when the on-screen controls are active.</summary>
        public static bool IsActive
        {
            get { return service != null; }
        }

        /// <summary>True while the layout editor is open.</summary>
        public static bool EditMode
        {
            get { return service != null && service.EditMode; }
        }

        /// <summary>
        /// Forces the controls to be active even when the engine is not in its
        /// touch mode. Used by the desktop build for testing.
        /// </summary>
        public static bool ForceEnable { get; set; }

        /// <summary>
        /// Creates the haptic service of the controls. The host can replace it
        /// to use the vibrator of the device.
        /// </summary>
        public static Func<IHapticService> HapticsFactory { get; set; }

        /// <summary>
        /// Creates the control system and hooks it into the screen of the game.
        /// </summary>
        /// <param name="game">The running game.</param>
        /// <param name="layers">The layers of the screen of the game.</param>
        public static void Initialize(SimitoneGame game, List<IGraphicsLayer> layers)
        {
            if (service != null || game == null) return;
            if (!FSOEnvironment.SoftwareKeyboard && !ForceEnable) return;
            if (layers == null) return;

            var device = game.GraphicsDevice;

            IHapticService haptics = NullHapticService.Instance;
            if (HapticsFactory != null)
            {
                try
                {
                    var created = HapticsFactory();
                    if (created != null) haptics = created;
                }
                catch (Exception)
                {
                    // Without haptics the controls still work.
                }
            }

            service = new TouchControlService(device, new MonoGameTouchBackend(), haptics);
            service.TextRenderer = CreateTextRenderer(game);

            // The profiles live next to the other user settings.
            try
            {
                var profileDirectory = Path.Combine(FSOEnvironment.UserDir ?? "Content/", "touch");
                Directory.CreateDirectory(profileDirectory);
                service.ProfileStore = new TouchControlProfileStore(profileDirectory);
                LoadStoredProfile(profileDirectory);
            }
            catch (Exception)
            {
                // Without a profile store the controls still work, they just
                // cannot be saved.
            }

            service.ActionTriggered += OnActionTriggered;

            inputLayer = new TouchInputLayer(service, GetScreenSize);
            overlayLayer = new TouchOverlayLayer(service);

            layers.Insert(0, inputLayer);
            layers.Add(overlayLayer);
        }

        /// <summary>Removes the control system from the game.</summary>
        public static void Shutdown()
        {
            if (service == null) return;

            service.ActionTriggered -= OnActionTriggered;
            service.Dispose();
            service = null;
            inputLayer = null;
            overlayLayer = null;
        }

        /// <summary>Opens or closes the layout editor.</summary>
        public static void ToggleEditMode()
        {
            if (service == null) return;
            service.EditMode = !service.EditMode;
        }

        /// <summary>Opens or closes the layout editor.</summary>
        public static void SetEditMode(bool active)
        {
            if (service == null) return;
            service.EditMode = active;
        }

        /// <summary>
        /// Called when the graphics device was reset, for example after a
        /// rotation of the screen.
        /// </summary>
        public static void OnDeviceReset()
        {
            if (service != null) service.OnDeviceReset();
        }

        /// <summary>
        /// The size of the screen the controls are drawn on.
        /// </summary>
        private static Vector2 GetScreenSize()
        {
            var game = GameFacade.Game;
            if (game != null && game.GraphicsDevice != null)
            {
                var viewport = game.GraphicsDevice.Viewport;
                return new Vector2(viewport.Width, viewport.Height);
            }
            return new Vector2(GlobalSettings.Default.GraphicsWidth, GlobalSettings.Default.GraphicsHeight);
        }

        /// <summary>
        /// Creates the renderer for the captions of the controls.
        /// </summary>
        private static ITouchTextRenderer CreateTextRenderer(SimitoneGame game)
        {
            try
            {
                var fieldFont = game.Content.Load<FieldFont>("../Fonts/mobile");
                if (fieldFont != null) return new SimitoneTextRenderer(fieldFont);
            }
            catch (Exception)
            {
                // Without a font the controls simply have no captions.
            }
            return NullTouchTextRenderer.Instance;
        }

        /// <summary>
        /// Loads the profile that was used the last time, or the default one.
        /// </summary>
        private static void LoadStoredProfile(string profileDirectory)
        {
            var store = service.ProfileStore;
            if (store == null) return;

            var active = service.Settings.ActiveProfile;
            if (!string.IsNullOrEmpty(active) && store.Exists(active))
            {
                var profile = store.Load(active);
                if (profile != null)
                {
                    profile.Name = active;
                    service.LoadProfile(profile);
                    return;
                }
            }

            // No profile yet - create the default one so that it can be saved.
            service.StoreLayoutIntoProfile();
            store.Save(service.Profile);
        }

        /// <summary>
        /// Reacts to the actions that do not have a keyboard equivalent.
        /// </summary>
        private static void OnActionTriggered(TouchAction action)
        {
            switch (action)
            {
                case TouchAction.EditControls:
                    ToggleEditMode();
                    return;
                case TouchAction.ModeLive:
                    SwitchMode(UIMainPanelMode.LIVE);
                    return;
                case TouchAction.ModeBuild:
                    SwitchMode(UIMainPanelMode.BUILD);
                    return;
                case TouchAction.ModeBuy:
                    SwitchMode(UIMainPanelMode.BUY);
                    return;
                case TouchAction.OpenOptions:
                case TouchAction.Menu:
                    SwitchMode(UIMainPanelMode.OPTIONS);
                    return;
            }

            var handler = HostActionRequested;
            if (handler != null) handler(action);
        }

        /// <summary>
        /// Switches the mode of the current game screen.
        /// </summary>
        private static void SwitchMode(UIMainPanelMode mode)
        {
            try
            {
                var screen = GameFacade.Screens?.CurrentUIScreen as TS1GameScreen;
                var switcher = screen?.Frontend?.ModeSwitcher;
                if (switcher != null)
                {
                    switcher.SwitchMode(mode);
                }
            }
            catch (Exception)
            {
                // The mode switch is a convenience, it must never break the game.
            }
        }
    }
}
