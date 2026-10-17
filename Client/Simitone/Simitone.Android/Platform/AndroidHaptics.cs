using System;
using Android.Content;
using Android.OS;
using Simitone.Touch.Input;

namespace Simitone.Android.Platform
{
    /// <summary>
    /// Haptic feedback through the Android vibrator service.
    /// </summary>
    public class AndroidHapticService : IHapticService
    {
        private readonly Vibrator vibrator;
        private readonly bool enabled;

        public AndroidHapticService(Context context, bool enabled)
        {
            this.enabled = enabled;
            try
            {
                vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            }
            catch (Exception)
            {
                vibrator = null;
            }
        }

        public void LightTick(float strength)
        {
            if (!enabled || vibrator == null) return;
            try
            {
                // A short, soft pulse.
                long duration = (long)(6 + 10 * Math.Max(0f, Math.Min(1f, strength)));
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    vibrator.Vibrate(VibrationEffect.CreateOneShot(duration, VibrationEffect.DefaultAmplitude));
                }
                else
                {
#pragma warning disable CS0618 // The old API is the only one available below API 26.
                    vibrator.Vibrate(duration);
#pragma warning restore CS0618
                }
            }
            catch (Exception)
            {
                // Haptics are a nice-to-have, never break the game over them.
            }
        }

        public void Confirm(float strength)
        {
            if (!enabled || vibrator == null) return;
            try
            {
                long duration = (long)(20 + 30 * Math.Max(0f, Math.Min(1f, strength)));
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    vibrator.Vibrate(VibrationEffect.CreateOneShot(duration, VibrationEffect.DefaultAmplitude));
                }
                else
                {
#pragma warning disable CS0618
                    vibrator.Vibrate(duration);
#pragma warning restore CS0618
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
