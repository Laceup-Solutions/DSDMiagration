using Android.Content;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Android;
using LaceupMigration;

[assembly: ExportRenderer(typeof(Shell), typeof(CustomTabBarRenderer))]
namespace LaceupMigration
{
    public class CustomTabBarRenderer : ShellRenderer
    {
        public CustomTabBarRenderer(Context context) : base(context)
        {
        }
        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        {
            return new CustomBottomNavAppearanceTracker(this, shellItem);
        }
    }


    public class CustomBottomNavAppearanceTracker : ShellBottomNavViewAppearanceTracker
    {
        public CustomBottomNavAppearanceTracker(IShellContext shellContext, ShellItem shellItem)
            : base(shellContext, shellItem)
        {
        }

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);

            bottomView.SetBackgroundColor(Android.Graphics.Color.White);
            bottomView.SetElevation(50);
        }
    }
}