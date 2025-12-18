#if ANDROID
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Android.Views;
using Android.Graphics.Drawables;
using AndroidGraphics = Android.Graphics;
using AndroidViews = Android.Views;

namespace LaceupMigration.Platforms.Android.Handlers
{
    public static class CollectionViewRippleMapper
    {
        public static void MapCollectionView(IViewHandler handler, IView view)
        {
            if (handler.PlatformView is RecyclerView recyclerView)
            {
                recyclerView.AddOnChildAttachStateChangeListener(new RippleAttachListener());
                
                // Also apply to existing items after a delay
                recyclerView.PostDelayed(() =>
                {
                    for (int i = 0; i < recyclerView.ChildCount; i++)
                    {
                        var child = recyclerView.GetChildAt(i);
                        RemoveOrangeRipple(child);
                    }
                }, 200);
            }
        }
        
        private static readonly AndroidGraphics.Color Gray100Color = AndroidGraphics.Color.ParseColor("#E1E1E1");
        
        private static void RemoveOrangeRipple(AndroidViews.View view)
        {
            try
            {
                if (view is AndroidViews.ViewGroup viewGroup)
                {
                    // Only apply to views that don't already have a background set
                    // This preserves borders and backgrounds on Frames, Borders, etc.
                    var currentBackground = viewGroup.Background;
                    bool hasBackground = currentBackground != null && 
                                        !(currentBackground is ColorDrawable colorDrawable && 
                                          colorDrawable.Color == AndroidGraphics.Color.Transparent);
                    
                    // Only apply StateListDrawable if no background is set (or it's transparent)
                    // This way we don't override borders and backgrounds on child views
                    if (!hasBackground)
                    {
                        // Apply Gray100 selection color instead of orange
                        // Create a StateListDrawable that uses Gray100 for selected state
                        var stateListDrawable = new StateListDrawable();
                        
                        // Selected state - Gray100
                        var selectedState = new int[] { global::Android.Resource.Attribute.StateSelected };
                        var selectedDrawable = new ColorDrawable(Gray100Color);
                        stateListDrawable.AddState(selectedState, selectedDrawable);
                        
                        // Pressed state - Gray100
                        var pressedState = new int[] { global::Android.Resource.Attribute.StatePressed };
                        var pressedDrawable = new ColorDrawable(Gray100Color);
                        stateListDrawable.AddState(pressedState, pressedDrawable);
                        
                        // Default state - Transparent
                        var defaultDrawable = new ColorDrawable(AndroidGraphics.Color.Transparent);
                        stateListDrawable.AddState(new int[] { }, defaultDrawable);
                        
                        viewGroup.Background = stateListDrawable;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
        
        private class RippleAttachListener : Java.Lang.Object, RecyclerView.IOnChildAttachStateChangeListener
        {
            public void OnChildViewAttachedToWindow(AndroidViews.View view)
            {
                view.Post(() => RemoveOrangeRipple(view));
            }
            
            public void OnChildViewDetachedFromWindow(AndroidViews.View view)
            {
                // Nothing to do
            }
        }
    }
}
#endif