using Android.App;
using Android.OS;
using Ctrip.Rider.Helpers;
using Firebase.Auth;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "@string/app_name", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class SplashActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		protected override void OnResume()
		{
			base.OnResume();

			FirebaseUser currentUser = AppDataHelper.GetCurrentUser();

			StartActivity(currentUser == null ? typeof(LoginActivity) : typeof(MainActivity));
		}
	}
}