using Android.App;
using Android.Content;
using Ctrip.Rider.Helpers;
using Firebase.Database;

namespace Ctrip.Rider.EventListeners
{
	public class UserProfileEventListener : Java.Lang.Object, IValueEventListener
	{
		private readonly ISharedPreferences _preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

		private ISharedPreferencesEditor _editor;

		public void OnCancelled(DatabaseError error)
		{

		}

		public void OnDataChange(DataSnapshot snapshot)
		{
			if (snapshot.Value == null)
			{
				return;
			}

			string fullName = snapshot.Child("fullname")?.Value?.ToString() ?? string.Empty;
			string email = snapshot.Child("email")?.Value?.ToString() ?? string.Empty;
			string phone = snapshot.Child("phone")?.Value?.ToString() ?? string.Empty;

			_editor.PutString("fullname", fullName);
			_editor.PutString("email", email);
			_editor.PutString("phone", phone);

			_editor.Apply();
		}

		public void Create()
		{
			_editor = _preferences.Edit();
			FirebaseDatabase database = AppDataHelper.GetDatabase();

			string userId = AppDataHelper.GetCurrentUser().Uid;
			DatabaseReference profileReference = database.GetReference("users/" + userId);

			profileReference.AddValueEventListener(this);
		}
	}
}