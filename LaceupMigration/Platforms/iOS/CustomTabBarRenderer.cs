using UIKit;
using CoreGraphics;
using System;
using LaceupMigration;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

[assembly: ExportRenderer(typeof(Shell), typeof(CustomTabBarRenderer))]
namespace LaceupMigration
{
    public class CustomTabBarRenderer : ShellRenderer
    {
        protected override IShellItemRenderer CreateShellItemRenderer(ShellItem item)
        {
            var renderer = base.CreateShellItemRenderer(item);

            if (renderer is ShellItemRenderer shellItemRenderer)
            {
                var tabBar = shellItemRenderer.TabBar;

                if (tabBar != null)
                {
                    // Apply shadow properties to the TabBar
                    tabBar.Layer.ShadowColor = UIColor.Black.CGColor;
                    tabBar.Layer.ShadowOffset = new CGSize(0, -4); // Shadow only on top
                    tabBar.Layer.ShadowOpacity = 0.25f; // Adjust for shadow intensity
                    tabBar.Layer.ShadowRadius = 4; // Adjust for blur
                    tabBar.Layer.MasksToBounds = false;

                    // Optional: Set a custom background color
                    tabBar.BackgroundColor = UIColor.White;
                }
            }

            return renderer;
        }
    }
}