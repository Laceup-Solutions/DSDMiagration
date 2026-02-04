



using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace LaceupMigration
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        const int BluetoothConnectRequestCode = 1;
        const int RequestStoragePermissionsCode = 100;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Apply theme overlay BEFORE base.OnCreate to ensure ripple color is set early
            if (Theme != null)
            {
                Theme.ApplyStyle(Resource.Style.LaceupThemeOverlay, true);
            }
            
            base.OnCreate(savedInstanceState);

            // Configure window to adjust for keyboard, allowing scrolling when keyboard is visible
            Window?.SetSoftInputMode(SoftInput.AdjustResize);

            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
            
            var permissionsToRequest = new List<string>();
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.BluetoothConnect) != (int)Permission.Granted)
                permissionsToRequest.Add(Android.Manifest.Permission.BluetoothConnect);
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
                permissionsToRequest.Add(Android.Manifest.Permission.WriteExternalStorage);
            if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
                permissionsToRequest.Add(Android.Manifest.Permission.ReadExternalStorage);
            // Required on Android 10+ for WifiManager.getConnectionInfo().getSSID() to return real SSID instead of "<unknown ssid>"
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q && ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) != (int)Permission.Granted)
                permissionsToRequest.Add(Android.Manifest.Permission.AccessFineLocation);

            if (permissionsToRequest.Count > 0)
            {
                ActivityCompat.RequestPermissions(this, permissionsToRequest.ToArray(), RequestStoragePermissionsCode);
            }
        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == BluetoothConnectRequestCode)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    // Permission granted, proceed with accessing bonded devices
                }
                else
                {
                    // Permission denied, handle accordingly
                }
            }
        }
    }
}
