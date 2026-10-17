using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using Simitone.Android.Platform;
using Simitone.Client;
using Simitone.Client.UI.Touch;
using Simitone.Touch;
using Simitone.Touch.Input;

namespace Simitone.Android
{
    /// <summary>
    /// The activity that hosts the game.
    /// </summary>
    /// <remarks>
    /// The activity does nothing but preparing the engine for Android and
    /// starting the game. Everything that is specific to Android lives in
    /// <see cref="SimitoneAndroidBootstrap"/>.
    /// </remarks>
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/Simitone.GameTheme",
        MainLauncher = false,
        LaunchMode = LaunchMode.SingleTop,
        HardwareAccelerated = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout |
                               ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Density |
                               ConfigChanges.KeyboardHidden | ConfigChanges.Keyboard,
        ScreenOrientation = ScreenOrientation.SensorLandscape
    )]
    public class MainActivity : AndroidGameActivity
    {
        private SimitoneGame game;
        private SimitoneAndroidSettings settings;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // The base implementation publishes the activity as
            // "Game.Activity", which has to happen before the game object is
            // created.
            base.OnCreate(savedInstanceState);

            settings = SimitoneAndroidSettings.Load(this);

            try
            {
                SimitoneAndroidBootstrap.Configure(this, settings);
            }
            catch (Exception ex)
            {
                ShowFatal("The game could not be prepared", ex);
                return;
            }

            // The on-screen controls use the vibrator of the device.
            TouchControlBridge.HapticsFactory = () => new AndroidHapticService(this, settings.Haptics);
            TouchControlBridge.HostActionRequested += OnHostActionRequested;

            try
            {
                game = new SimitoneGame();
                // The Android platform runs the game asynchronously, the
                // "Run" method of the client is hidden for the desktop build.
                ((Game)game).Run();
            }
            catch (Exception ex)
            {
                ShowFatal("The game could not be started", ex);
            }
        }

        protected override void OnDestroy()
        {
            TouchControlBridge.HostActionRequested -= OnHostActionRequested;
            base.OnDestroy();
        }

        /// <summary>
        /// Handles the actions of the on-screen controls that the engine does
        /// not know about.
        /// </summary>
        private void OnHostActionRequested(TouchAction action)
        {
            try
            {
                switch (action)
                {
                    case TouchAction.Menu:
                        // Open the setup screen so that the user can change the
                        // resolution, the folder and the other options.
                        StartActivity(new Intent(this, typeof(LauncherActivity)));
                        break;
                    case TouchAction.OpenInventory:
                    case TouchAction.OpenFamily:
                    case TouchAction.OpenSkills:
                    case TouchAction.FreeCamera:
                        ShowToast("This control is not available yet.");
                        break;
                }
            }
            catch (Exception)
            {
                // Never break the game because of a button.
            }
        }

        /// <summary>
        /// The back button closes the layout editor first and only then leaves
        /// the game.
        /// </summary>
        public override void OnBackPressed()
        {
            if (TouchControlBridge.IsActive && TouchControlBridge.EditMode)
            {
                TouchControlBridge.SetEditMode(false);
                return;
            }

            base.OnBackPressed();
        }

        private void ShowToast(string message)
        {
            try
            {
                var toast = Toast.MakeText(this, message, ToastLength.Short);
                toast?.Show();
            }
            catch (Exception)
            {
            }
        }

        private void ShowFatal(string title, Exception ex)
        {
            try
            {
                Android.Util.Log.Error("Simitone", title + ": " + ex);
            }
            catch (Exception)
            {
            }

            try
            {
                new AlertDialog.Builder(this)
                    .SetTitle(title)
                    .SetMessage(ex.ToString())
                    .SetPositiveButton("Close", (EventHandler<DialogClickEventArgs>)((s, e) => { }))
                    .Show();
            }
            catch (Exception)
            {
            }
        }
    }
}
