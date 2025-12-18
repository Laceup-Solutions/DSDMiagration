using Microsoft.Maui.Controls;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

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
                    // Create ripple drawable with Gray100 (#E1E1E1) for selection highlight
                    var gray100Color = global::Android.Graphics.Color.ParseColor("#E1E1E1");
                    var colorStateList = global::Android.Content.Res.ColorStateList.ValueOf(gray100Color);
                    var contentDrawable = new global::Android.Graphics.Drawables.ColorDrawable(global::Android.Graphics.Color.Transparent);
                    var rippleDrawable = new global::Android.Graphics.Drawables.RippleDrawable(colorStateList, contentDrawable, null);
                    
                    // Apply to the view group itself but DON'T make it clickable/focusable
                    // This allows touch events to pass through to child views (Frame with TapGestureRecognizer)
                    // or to the CollectionView's selection mechanism
                    viewGroup.Background = rippleDrawable;
                    // Don't set Clickable/Focusable - let the child views or CollectionView handle touches
                    
                    // Use ViewTreeObserver to apply ripple after view is attached
                    var observer = viewGroup.ViewTreeObserver;
                    if (observer != null && observer.IsAlive)
                    {
                        observer.AddOnGlobalLayoutListener(new RippleLayoutListener(viewGroup, rippleDrawable));
                    }
                    
                    // Also try to find and apply to RecyclerView item container
                    FindAndApplyToRecyclerViewItem(viewGroup, rippleDrawable);
                }
                catch
                {
                    // Ignore errors
                }
            }
        }
        
        private static void FindAndApplyToRecyclerViewItem(global::Android.Views.ViewGroup viewGroup, global::Android.Graphics.Drawables.RippleDrawable rippleDrawable)
        {
            try
            {
                // Traverse up to find the RecyclerView item container
                var current = viewGroup;
                for (int i = 0; i < 5 && current != null; i++)
                {
                    // Check if this view or its parent is the item container
                    if (current.Clickable || current.Focusable)
                    {
                        // This might be the item container - apply ripple
                        current.Background = rippleDrawable;
                    }
                    
                    // Check parent
                    if (current.Parent is global::Android.Views.ViewGroup parent)
                    {
                        // If parent looks like a RecyclerView item container, apply ripple
                        if (parent.Clickable || (parent.ChildCount > 0 && parent.GetChildAt(0) == current))
                        {
                            parent.Background = rippleDrawable;
                        }
                        current = parent;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
        
#if ANDROID
        private class RippleLayoutListener : Java.Lang.Object, global::Android.Views.ViewTreeObserver.IOnGlobalLayoutListener
        {
            private readonly global::Android.Views.ViewGroup _viewGroup;
            private readonly global::Android.Graphics.Drawables.RippleDrawable _rippleDrawable;
            
            public RippleLayoutListener(global::Android.Views.ViewGroup viewGroup, global::Android.Graphics.Drawables.RippleDrawable rippleDrawable)
            {
                _viewGroup = viewGroup;
                _rippleDrawable = rippleDrawable;
            }
            
            public void OnGlobalLayout()
            {
                try
                {
                    // Apply ripple to parent containers after layout
                    if (_viewGroup.Parent is global::Android.Views.ViewGroup parent)
                    {
                        if (parent.Clickable || parent.Focusable)
                        {
                            parent.Background = _rippleDrawable;
                        }
                    }
                    
                    // Remove listener after first layout
                    var observer = _viewGroup.ViewTreeObserver;
                    if (observer != null && observer.IsAlive)
                    {
                        observer.RemoveOnGlobalLayoutListener(this);
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }
        }
#endif
#endif
    }
}

