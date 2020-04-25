using Android.App;
using Android.Content;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

namespace Ctrip.Rider.Helpers
{
	public static class AppDataHelper
	{
		private static readonly ISharedPreferences Preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

		public static FirebaseDatabase GetDatabase()
		{
			FirebaseApp app = FirebaseApp.InitializeApp(Application.Context);

			if (app == null)
			{
				FirebaseApp.InitializeApp(Application.Context, FirebaseBuilder.BuildOptions());
			}

			return FirebaseDatabase.GetInstance(app);
		}


		public static FirebaseAuth GetFirebaseAuth()
		{
			FirebaseApp app = FirebaseApp.InitializeApp(Application.Context);

			if (app == null)
			{
				FirebaseApp.InitializeApp(Application.Context, FirebaseBuilder.BuildOptions());
			}

			return FirebaseAuth.Instance;
		}

		public static FirebaseUser GetCurrentUser()
		{
			FirebaseApp app = FirebaseApp.InitializeApp(Application.Context);

			if (app == null)
			{
				FirebaseApp.InitializeApp(Application.Context, FirebaseBuilder.BuildOptions());
			}

			return FirebaseAuth.Instance.CurrentUser;
		}

		public static string GetFullName()
		{
			return Preferences.GetString("fullname", string.Empty);
		}

		public static string GetEmail()
		{
			return Preferences.GetString("email", string.Empty);
		}

		public static string GetPhone()
		{
			return Preferences.GetString("phone", string.Empty);
		}
	}
}