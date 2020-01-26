using Android.Content;
using Firebase;

namespace Ctrip.Rider.Helpers
{
	public static class FirebaseBuilder
	{
		public static FirebaseOptions BuildOptions()
		{
			return new FirebaseOptions.Builder()
				.SetApplicationId("ctrip-50eab")
				.SetApiKey("AIzaSyDBk-f9zqpg1uGZYAHUt5kV8xbOxGQiS9w")
				.SetDatabaseUrl("https://ctrip-50eab.firebaseio.com")
				.SetStorageBucket("ctrip-50eab.appspot.com")
				.Build();
		}
	}
}