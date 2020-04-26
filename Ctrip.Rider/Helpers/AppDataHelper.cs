using Android.App;
using Android.Content;
using Ctrip.Rider.Activities;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Java.Util.Concurrent;

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

		public static void SendVerificationCode(string numeroCelular, PhoneValidationActivity instance)
		{
			GetDatabase();

			PhoneVerificationCallback phoneAuthCallbacks = new PhoneVerificationCallback(instance);

			FirebaseAuth auth = GetFirebaseAuth();

			PhoneAuthProvider.GetInstance(auth).VerifyPhoneNumber(numeroCelular, 30, TimeUnit.Seconds, instance, phoneAuthCallbacks);
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

		public static string GetIntFormat()
		{
			return Preferences.GetString("int_format", string.Empty);
		}
	}
}