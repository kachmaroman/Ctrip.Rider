using Android.App;
using Android.Support.V7.App;
using Ctrip.Rider.Fragments;
using Ctrip.Rider.Helpers;
using Firebase.Auth;
using Plugin.Connectivity;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "@string/app_name", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class SplashActivity : AppCompatActivity
	{
		protected override void OnResume()
		{
			base.OnResume();

			FirebaseUser currentUser = AppDataHelper.GetCurrentUser();

			if (!CrossConnectivity.Current.IsConnected)
			{
				NoNetworkFragment.Display(SupportFragmentManager);
				return;
			}

			StartActivity(currentUser != null ? typeof(MainActivity) : typeof(OnboardingActivity));
		}
	}
}