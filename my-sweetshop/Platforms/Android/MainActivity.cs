using Android.App;
using Android.OS;
using Android.Views;

namespace my_sweetshop;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            Window.SetStatusBarColor(Android.Graphics.Color.White);

            Window.DecorView.SystemUiVisibility =
                (StatusBarVisibility)SystemUiFlags.LightStatusBar;
        }
    }
}
