using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace PhotoboothParty;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            Window?.SetDecorFitsSystemWindows(false);
            if (Window?.InsetsController != null)
            {
                Window.InsetsController.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                Window.InsetsController.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
            }
        }
        else
        {
#pragma warning disable CS0618
            if (Window != null)
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                    SystemUiFlags.ImmersiveSticky |
                    SystemUiFlags.HideNavigation |
                    SystemUiFlags.Fullscreen |
                    SystemUiFlags.LayoutHideNavigation |
                    SystemUiFlags.LayoutFullscreen |
                    SystemUiFlags.LayoutStable);
            }
#pragma warning restore CS0618
        }
    }

    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (e != null && e.Action == KeyEventActions.Down)
        {
            var keyCode = e.KeyCode;
            if (keyCode == Keycode.VolumeUp || keyCode == Keycode.VolumeDown || keyCode == Keycode.Enter)
            {
                WeakReferenceMessenger.Default.Send(new ShutterPressedMessage());
                return true; 
            }
        }
        return base.DispatchKeyEvent(e);
    }
}
