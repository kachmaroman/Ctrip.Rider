using Android.App;
using Android.OS;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "@string/app_name", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class SplashActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here
		}

		protected override void OnResume()
		{
			base.OnResume();
			StartActivity(typeof(LoginActivity));
		}
	}
}