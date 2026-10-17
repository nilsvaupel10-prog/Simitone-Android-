using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Views;
using Android.Widget;
using Simitone.Android.Platform;

namespace Simitone.Android
{
    /// <summary>
    /// The setup screen of the Android port.
    /// </summary>
    /// <remarks>
    /// The screen lets the user pick the folder of their copy of
    /// "The Sims: Complete Collection", choose the resolution, the graphics
    /// backend and the UI scale, and finally start the game.
    /// It is shown on the first start and can be reopened from the game
    /// (pause menu / options).
    /// </remarks>
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Theme = "@style/Simitone.LauncherTheme",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout |
                               ConfigChanges.SmallestScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.UiMode |
                               ConfigChanges.Density | ConfigChanges.KeyboardHidden,
        ScreenOrientation = ScreenOrientation.SensorLandscape
    )]
    public class LauncherActivity : Activity
    {
        private const int RequestCodeFolder = 4711;
        private const int RequestCodeAllFiles = 4712;
        private const int RequestCodePermissions = 4713;

        private SimitoneAndroidSettings _settings;

        private TextView _pathLabel;
        private TextView _statusLabel;
        private TextView _uiScaleDescription;
        private EditText _manualPath;
        private EditText _customWidth;
        private EditText _customHeight;
        private Spinner _resolutionSpinner;
        private Spinner _graphicsSpinner;
        private Spinner _uiScaleSpinner;
        private CheckBox _enable3D;
        private CheckBox _haptics;
        private LinearLayout _customResolutionRow;

        /// <summary>True when the game should be started automatically.</summary>
        private bool _autoLaunch;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _settings = SimitoneAndroidSettings.Load(this);
            _autoLaunch = Intent != null && Intent.GetBooleanExtra("autoLaunch", false);

            BuildUi();
            RefreshValues();
            UpdatePathStatus();

            if (_autoLaunch && TryLaunch())
            {
                Finish();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            // The user may have granted the "all files access" permission while
            // the app was in the background.
            if (_settings != null)
            {
                var path = AndroidGamePath.GetStoredPath(this);
                if (!string.IsNullOrEmpty(path) && path != _settings.GamePath)
                {
                    _settings.GamePath = path;
                    _pathLabel.Text = path;
                    UpdatePathStatus();
                }
            }
        }

        // ---------------------------------------------------------------- UI --

        private void BuildUi()
        {
            var scroll = new ScrollView(this);
            var root = new LinearLayout(this) { Orientation = Orientation.Vertical };
            root.SetPadding(Dp(20), Dp(16), Dp(20), Dp(24));
            scroll.AddView(root);

            var title = new TextView(this) { Text = "Simitone" };
            title.SetTextSize(ComplexTypeUnitType.Sp, 28);
            title.SetTextColor(Color.White);
            title.SetTypeface(Typeface.Create("sans-serif", TypefaceStyle.Bold));
            root.AddView(title);

            var subtitle = new TextView(this) { Text = "Android port - initial setup" };
            subtitle.SetTextSize(ComplexTypeUnitType.Sp, 14);
            subtitle.SetTextColor(Color.Rgb(150, 160, 175));
            root.AddView(subtitle, new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MatchParent,
                LinearLayout.LayoutParams.WrapContent));

            root.AddView(Spacer(18));

            // --- game files ---------------------------------------------------
            var filesCard = CreateCard("Game files");
            filesCard.AddView(CreateLabel("The Sims: Complete Collection folder"));
            filesCard.AddView(CreateHint("Simitone needs your own copy of the game. " +
                                         "No game data is bundled with the app."));

            _pathLabel = new TextView(this) { Text = "No folder selected yet." };
            _pathLabel.SetTextSize(ComplexTypeUnitType.Sp, 13);
            _pathLabel.SetTextColor(Color.Rgb(210, 220, 235));
            _pathLabel.SetTypeface(Typeface.Create("monospace", TypefaceStyle.Normal));
            _pathLabel.SetPadding(Dp(10), Dp(10), Dp(10), Dp(10));
            _pathLabel.SetBackground(Rounded(Color.Argb(40, 0, 0, 0), 8));
            filesCard.AddView(_pathLabel);

            _statusLabel = new TextView(this) { Text = "" };
            _statusLabel.SetTextSize(ComplexTypeUnitType.Sp, 13);
            _statusLabel.SetPadding(Dp(2), Dp(8), Dp(2), Dp(4));
            filesCard.AddView(_statusLabel);

            var chooseButton = CreateButton("Choose folder...");
            chooseButton.Click += (s, e) => PickFolder();
            filesCard.AddView(chooseButton);

            var manualButton = CreateButton("Enter the path manually");
            manualButton.Click += (s, e) => ToggleManualPath();
            filesCard.AddView(manualButton);

            _manualPath = new EditText(this)
            {
                Hint = "/storage/emulated/0/The Sims",
                SingleLine = true
            };
            _manualPath.SetTextColor(Color.White);
            _manualPath.SetHintTextColor(Color.Rgb(120, 130, 145));
            _manualPath.SetBackground(Rounded(Color.Argb(40, 0, 0, 0), 8));
            _manualPath.Visibility = ViewStates.Gone;
            filesCard.AddView(_manualPath);

            var applyManual = CreateButton("Use this path");
            applyManual.Visibility = ViewStates.Gone;
            applyManual.Click += (s, e) => ApplyManualPath();
            filesCard.AddView(applyManual);

            var permissionButton = CreateButton("Grant access to all files");
            permissionButton.Click += (s, e) => RequestAllFilesAccess();
            filesCard.AddView(permissionButton);

            root.AddView(filesCard);
            root.AddView(Spacer(14));

            // --- display ------------------------------------------------------
            var displayCard = CreateCard("Display");
            displayCard.AddView(CreateLabel("Resolution"));
            displayCard.AddView(CreateHint("The internal (logical) width of the game. " +
                                           "The height follows the aspect ratio of the device."));

            _resolutionSpinner = new Spinner(this);
            _resolutionSpinner.Adapter = new DarkSpinnerAdapter(this, SimitoneAndroidSettings.ResolutionNames);
            _resolutionSpinner.ItemSelected += (s, e) => OnResolutionSelected();
            displayCard.AddView(_resolutionSpinner);

            _customResolutionRow = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            _customWidth = CreateNumberField("Width");
            _customHeight = CreateNumberField("Height");
            _customResolutionRow.AddView(_customWidth, new LinearLayout.LayoutParams(0, Dp(48), 1f));
            _customResolutionRow.AddView(_customHeight, new LinearLayout.LayoutParams(0, Dp(48), 1f));
            _customResolutionRow.Visibility = ViewStates.Gone;
            displayCard.AddView(_customResolutionRow);

            displayCard.AddView(CreateLabel("Graphics backend"));
            _graphicsSpinner = new Spinner(this);
            _graphicsSpinner.Adapter = new DarkSpinnerAdapter(this, SimitoneAndroidSettings.GraphicsNames);
            _graphicsSpinner.ItemSelected += (s, e) => OnGraphicsSelected();
            displayCard.AddView(_graphicsSpinner);

            _enable3D = new CheckBox(this) { Text = "Enable 3D mode", Checked = true };
            _enable3D.SetTextColor(Color.White);
            _enable3D.SetTextSize(ComplexTypeUnitType.Sp, 15);
            displayCard.AddView(_enable3D);

            displayCard.AddView(CreateLabel("Interface scale"));
            _uiScaleSpinner = new Spinner(this);
            _uiScaleSpinner.Adapter = new DarkSpinnerAdapter(this, SimitoneAndroidSettings.UiScaleNames);
            _uiScaleSpinner.ItemSelected += (s, e) => OnUiScaleSelected();
            displayCard.AddView(_uiScaleSpinner);

            _uiScaleDescription = CreateHint("");
            displayCard.AddView(_uiScaleDescription);

            _haptics = new CheckBox(this) { Text = "Haptic feedback", Checked = true };
            _haptics.SetTextColor(Color.White);
            _haptics.SetTextSize(ComplexTypeUnitType.Sp, 15);
            displayCard.AddView(_haptics);

            root.AddView(displayCard);
            root.AddView(Spacer(18));

            // --- launch -------------------------------------------------------
            var launchButton = CreatePrimaryButton("Launch game");
            launchButton.Click += (s, e) => TryLaunch();
            root.AddView(launchButton);

            var resetButton = CreateButton("Reset the setup");
            resetButton.Click += (s, e) => ResetSetup();
            root.AddView(resetButton);

            var about = CreateHint("Simitone is a free client for The Sims 1. " +
                                   "You need a legitimate copy of The Sims: Complete Collection to play.");
            about.SetPadding(0, Dp(14), 0, 0);
            root.AddView(about);

            SetContentView(scroll);
        }

        private void RefreshValues()
        {
            _resolutionSpinner.SetSelection((int)_settings.Resolution);
            _graphicsSpinner.SetSelection((int)_settings.Graphics);
            _uiScaleSpinner.SetSelection((int)_settings.UiScale);
            _enable3D.Checked = _settings.Enable3D;
            _haptics.Checked = _settings.Haptics;
            _customWidth.Text = _settings.CustomWidth.ToString(CultureInfo.InvariantCulture);
            _customHeight.Text = _settings.CustomHeight.ToString(CultureInfo.InvariantCulture);
            _pathLabel.Text = string.IsNullOrEmpty(_settings.GamePath)
                ? "No folder selected yet."
                : _settings.GamePath;
        }

        private void OnResolutionSelected()
        {
            var preset = (ResolutionPreset)_resolutionSpinner.SelectedItemPosition;
            _customResolutionRow.Visibility = preset == ResolutionPreset.Custom
                ? ViewStates.Visible
                : ViewStates.Gone;
        }

        private void OnGraphicsSelected()
        {
            _settings.Graphics = (GraphicsMode)_graphicsSpinner.SelectedItemPosition;
        }

        private void OnUiScaleSelected()
        {
            _settings.UiScale = (UiScaleOption)_uiScaleSpinner.SelectedItemPosition;
            _uiScaleDescription.Text = _settings.UiScaleDescription;
        }

        // ---------------------------------------------------------- behaviour --

        private void PickFolder()
        {
            try
            {
                var intent = new Intent(Intent.ActionOpenDocumentTree);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantPersistableUriPermission);
#pragma warning disable CS0618 // The Activity Result API is not available on plain Activities.
                StartActivityForResult(intent, RequestCodeFolder);
#pragma warning restore CS0618
            }
            catch (Exception ex)
            {
                ShowMessage("Folder picker", "The folder picker could not be opened: " + ex.Message);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
#pragma warning disable CS0618 // The Activity Result API needs a ComponentActivity.
            base.OnActivityResult(requestCode, resultCode, data);
#pragma warning restore CS0618

            if (requestCode != RequestCodeFolder) return;
            if (resultCode != Result.Ok || data == null || data.Data == null) return;

            var uri = data.Data;

            try
            {
                // Keep the permission so that the folder is still known after a
                // restart of the device.
                ContentResolver.TakePersistableUriPermission(uri, ActivityFlags.GrantReadUriPermission);
            }
            catch (Exception)
            {
                // Not all providers support persistable permissions.
            }

            var path = AndroidGamePath.ResolveTreeToPath(this, uri);
            if (string.IsNullOrEmpty(path))
            {
                ShowMessage("Folder picker",
                    "The selected folder could not be mapped to a path on this device.\n\n" +
                    "Please use \"Enter the path manually\" and type the location of your " +
                    "The Sims installation (for example /storage/emulated/0/The Sims).");
                return;
            }

            _settings.GamePath = path;
            _settings.TreeUri = uri.ToString() ?? "";
            _pathLabel.Text = path;
            UpdatePathStatus();
        }

        private void ToggleManualPath()
        {
            if (_manualPath.Visibility == ViewStates.Visible)
            {
                _manualPath.Visibility = ViewStates.Gone;
                return;
            }

            _manualPath.Visibility = ViewStates.Visible;
            _manualPath.Text = _settings.GamePath;
        }

        private void ApplyManualPath()
        {
            var path = (_manualPath.Text ?? "").Trim();
            if (path.Length == 0)
            {
                ShowMessage("Invalid path", "Please enter the path of your The Sims installation.");
                return;
            }

            _settings.GamePath = path;
            _pathLabel.Text = path;
            UpdatePathStatus();
        }

        private void UpdatePathStatus()
        {
            var validation = AndroidGamePath.Validate(_settings.GamePath);

            if (validation.IsValid)
            {
                _statusLabel.SetTextColor(Color.Rgb(120, 220, 140));
                _statusLabel.Text = "OK - " + validation.Message;
            }
            else
            {
                _statusLabel.SetTextColor(Color.Rgb(240, 120, 120));
                _statusLabel.Text = validation.Message;
            }
        }

        private bool TryLaunch()
        {
            var validation = AndroidGamePath.Validate(_settings.GamePath);
            if (!validation.IsValid)
            {
                UpdatePathStatus();
                ShowMessage("Game data not found",
                    validation.Message + "\n\n" +
                    "Select the folder that contains the \"GameData\" directory of your " +
                    "The Sims: Complete Collection installation.");
                return false;
            }

            if (!HasAllFilesAccess())
            {
                ShowMessage("Permission needed",
                    "Simitone reads the game files directly from the storage, so Android needs the " +
                    "\"All files access\" permission.\n\nTap \"Grant access to all files\" to grant it.");
                return false;
            }

            // Store the settings before the game activity reads them.
            _settings.Resolution = (ResolutionPreset)_resolutionSpinner.SelectedItemPosition;
            _settings.Graphics = (GraphicsMode)_graphicsSpinner.SelectedItemPosition;
            _settings.UiScale = (UiScaleOption)_uiScaleSpinner.SelectedItemPosition;
            _settings.Enable3D = _enable3D.Checked;
            _settings.Haptics = _haptics.Checked;
            _settings.CustomWidth = ParseNumber(_customWidth.Text, _settings.CustomWidth);
            _settings.CustomHeight = ParseNumber(_customHeight.Text, _settings.CustomHeight);
            _settings.Save(this);
            AndroidGamePath.MarkSetupComplete(this);

            try
            {
                StartActivity(new Intent(this, typeof(MainActivity)));
            }
            catch (Exception ex)
            {
                ShowMessage("Error", "The game could not be started: " + ex.Message);
                return false;
            }

            return true;
        }

        private void ResetSetup()
        {
            AndroidGamePath.ClearSetup(this);
            _settings = new SimitoneAndroidSettings();
            RefreshValues();
            UpdatePathStatus();
            ShowMessage("Setup reset", "All settings have been reset.");
        }

        private int ParseNumber(string text, int fallback)
        {
            int value;
            if (!string.IsNullOrWhiteSpace(text) && int.TryParse(text.Trim(), out value) && value > 0)
            {
                return value;
            }
            return fallback;
        }

        private bool HasAllFilesAccess()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.R) return true;
            try
            {
                return Environment.IsExternalStorageManager;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private void RequestAllFilesAccess()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.R)
            {
                RequestLegacyStoragePermissions();
                return;
            }

            try
            {
                var intent = new Intent();
                intent.SetAction("android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION");
                intent.SetData(Uri.Parse("package:" + PackageName));
                StartActivity(intent);
            }
            catch (Exception)
            {
                try
                {
                    StartActivity(new Intent("android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION"));
                }
                catch (Exception ex)
                {
                    ShowMessage("Permission", "The permission screen could not be opened: " + ex.Message);
                }
            }
        }

        private void RequestLegacyStoragePermissions()
        {
            try
            {
                var permissions = new List<string>
                {
                    Android.Manifest.Permission.ReadExternalStorage,
                    Android.Manifest.Permission.WriteExternalStorage
                }.ToArray();

                RequestPermissions(permissions, RequestCodePermissions);
            }
            catch (Exception)
            {
                // Nothing to do - the game will report the problem later on.
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == RequestCodePermissions)
            {
                UpdatePathStatus();
            }
        }

        // ------------------------------------------------------------ helpers --

        private void ShowMessage(string title, string message)
        {
            try
            {
                new AlertDialog.Builder(this)
                    .SetTitle(title)
                    .SetMessage(message)
                    .SetPositiveButton("OK", (EventHandler<DialogClickEventArgs>)((s, e) => { }))
                    .Show();
            }
            catch (Exception)
            {
                // There is no window (yet) - ignore.
            }
        }

        private TextView CreateLabel(string text)
        {
            var view = new TextView(this) { Text = text };
            view.SetTextSize(ComplexTypeUnitType.Sp, 15);
            view.SetTextColor(Color.White);
            view.SetTypeface(Typeface.Create("sans-serif-medium", TypefaceStyle.Normal));
            view.SetPadding(0, Dp(10), 0, Dp(2));
            return view;
        }

        private TextView CreateHint(string text)
        {
            var view = new TextView(this) { Text = text };
            view.SetTextSize(ComplexTypeUnitType.Sp, 12);
            view.SetTextColor(Color.Rgb(140, 150, 165));
            return view;
        }

        private Button CreateButton(string text)
        {
            var button = new Button(this) { Text = text };
            button.SetTextSize(ComplexTypeUnitType.Sp, 14);
            button.SetTextColor(Color.White);
            button.SetBackground(Rounded(Color.Argb(60, 255, 255, 255), 10));
            button.SetAllCaps(false);
            button.LayoutParameters = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MatchParent, Dp(48));
            button.SetPadding(Dp(12), 0, Dp(12), 0);
            return button;
        }

        private Button CreatePrimaryButton(string text)
        {
            var button = CreateButton(text);
            button.SetTextSize(ComplexTypeUnitType.Sp, 17);
            button.SetTypeface(Typeface.Create("sans-serif-medium", TypefaceStyle.Normal));
            button.SetBackground(Rounded(Color.Rgb(58, 130, 246), 12));
            button.LayoutParameters = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MatchParent, Dp(58));
            return button;
        }

        private EditText CreateNumberField(string hint)
        {
            var edit = new EditText(this) { Hint = hint, SingleLine = true };
            edit.InputType = Android.Text.InputTypes.ClassNumber;
            edit.SetTextColor(Color.White);
            edit.SetHintTextColor(Color.Rgb(120, 130, 145));
            edit.SetBackground(Rounded(Color.Argb(40, 0, 0, 0), 8));
            edit.SetPadding(Dp(10), 0, Dp(10), 0);
            return edit;
        }

        private LinearLayout CreateCard(string title)
        {
            var card = new LinearLayout(this) { Orientation = Orientation.Vertical };
            card.SetPadding(Dp(14), Dp(8), Dp(14), Dp(14));
            card.SetBackground(Rounded(Color.Argb(28, 255, 255, 255), 14));

            var header = new TextView(this) { Text = title };
            header.SetTextSize(ComplexTypeUnitType.Sp, 17);
            header.SetTextColor(Color.Rgb(120, 170, 255));
            header.SetTypeface(Typeface.Create("sans-serif-medium", TypefaceStyle.Normal));
            card.AddView(header);

            return card;
        }

        private View Spacer(int height)
        {
            return new View(this) { LayoutParameters = new LinearLayout.LayoutParams(1, Dp(height)) };
        }

        private GradientDrawable Rounded(Color color, int radius)
        {
            var drawable = new GradientDrawable();
            drawable.SetColor(color);
            drawable.SetCornerRadius(Dp(radius));
            return drawable;
        }

        private int Dp(int value)
        {
            return (int)(value * Resources.DisplayMetrics.Density + 0.5f);
        }
    }

    /// <summary>
    /// A spinner adapter that stays readable on the dark background.
    /// </summary>
    internal class DarkSpinnerAdapter : ArrayAdapter<string>
    {
        public DarkSpinnerAdapter(Context context, string[] items)
            : base(context, Android.Resource.Layout.SimpleSpinnerItem, items)
        {
            SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = base.GetView(position, convertView, parent);
            var text = view as TextView;
            if (text != null)
            {
                text.SetTextColor(Color.White);
                text.SetTextSize(ComplexTypeUnitType.Sp, 15);
            }
            return view;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            var view = base.GetDropDownView(position, convertView, parent);
            var text = view as TextView;
            if (text != null)
            {
                text.SetTextColor(Color.White);
                text.SetBackgroundColor(Color.Rgb(32, 36, 44));
            }
            return view;
        }
    }
}
