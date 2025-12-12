using Microsoft.Maui.Controls;

namespace LaceupMigration.Controls
{
    public static class CollectionViewRippleHelper
    {
        public static readonly BindableProperty EnableRippleProperty =
            BindableProperty.CreateAttached(
                "EnableRipple",
                typeof(bool),
                typeof(CollectionViewRippleHelper),
                false,
                propertyChanged: OnEnableRippleChanged);
        
        public static bool GetEnableRipple(BindableObject view)
        {
            return (bool)view.GetValue(EnableRippleProperty);
        }
        
        public static void SetEnableRipple(BindableObject view, bool value)
        {
            view.SetValue(EnableRippleProperty, value);
        }
        
        private static void OnEnableRippleChanged(BindableObject bindable, object oldValue, object newValue)
        {
#if ANDROID
            if (bindable is View view && newValue is bool enable && enable)
            {
                view.HandlerChanged += OnHandlerChanged;
                
                // If handler is already available, apply immediately
                if (view.Handler != null)
                {
                    ApplyRipple(view);
                }
            }
#endif
        }
        
#if ANDROID
        private static void OnHandlerChanged(object sender, EventArgs e)
        {
            if (sender is View view)
            {
                ApplyRipple(view);
            }
        }
        
        private static void ApplyRipple(View view)
        {
            if (view.Handler?.PlatformView is global::Android.Views.ViewGroup viewGroup)
            {
                try
                {
                    using (var colorStateList = global::Android.Content.Res.ColorStateList.ValueOf(global::Android.Graphics.Color.ParseColor("#b6e3f1")))
                    {
                        var contentDrawable = new global::Android.Graphics.Drawables.ColorDrawable(global::Android.Graphics.Color.Transparent);
                        var rippleDrawable = new global::Android.Graphics.Drawables.RippleDrawable(colorStateList, contentDrawable, null);
                        viewGroup.Background = rippleDrawable;
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }
        }
#endif
    }
}

