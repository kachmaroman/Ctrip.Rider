using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Ctrip.Rider.Activities;
using Ctrip.Rider.Helpers;
using Firebase.Auth;
using Plugin.Connectivity;

namespace Ctrip.Rider.Fragments
{
	public class NoNetworkFragment : Android.Support.V4.App.DialogFragment
	{
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetStyle(StyleNormal, Resource.Style.FullScreenDG);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.no_network_layout, container, false);
			RelativeLayout parentRoot = view.FindViewById<RelativeLayout>(Resource.Id.no_net_root);
			Button tryBtn = view.FindViewById<Button>(Resource.Id.no_net_prim_btn);

			tryBtn.Click += (s, e) =>
			{
				if (!CrossConnectivity.Current.IsConnected)
				{
					Snackbar snackbar = Snackbar.Make(parentRoot,
							"We can't reach our network right now. Please check your connection",
							Snackbar.LengthIndefinite)
						.SetAction("Dismiss", v => { });

					snackbar.View.SetBackgroundColor(Color.Red);
					snackbar.Show();

					return;
				}

				FirebaseUser currentUser = AppDataHelper.GetCurrentUser();
				Intent intent = new Intent(Activity, currentUser != null ? typeof(MainActivity) : typeof(OnboardingActivity));

				StartActivity(intent);
			};

			return view;
		}

		public static NoNetworkFragment Display(Android.Support.V4.App.FragmentManager manager)
		{
			NoNetworkFragment noNetworkFragment = new NoNetworkFragment
			{
				Cancelable = false
			};

			noNetworkFragment.Show(manager, "network_connection");

			return noNetworkFragment;
		}
	}
}