using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Mukesh.CountryPickerLib;
using Firebase.Auth;
using Firebase.Database;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using Ctrip.Rider.Constants;
using Ctrip.Rider.DataModels;
using Ctrip.Rider.EventListeners;
using Ctrip.Rider.Helpers;
using Java.Util;
using Org.Json;
using static Xamarin.Facebook.GraphRequest;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Result = Android.App.Result;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "OnboardingActivity", Theme = "@style/AppTheme.MainScreen", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, ScreenOrientation = ScreenOrientation.Portrait)]
	public class OnboardingActivity : AppCompatActivity, IFacebookCallback, IValueEventListener, IOnFailureListener, IOnSuccessListener, IGraphJSONObjectCallback
	{
		private RelativeLayout _mRelativeLayout;
		private LinearLayout _mLinearLayout;
		private EditText _mEditText;
		private CircleImageView _countryFlagImg;
		private FloatingActionButton _mGoogleFab, _mFacebookFab;
		private CookieBarHelper _helper;

		private bool _usingFirebase;
		public const int RequestPermission = 200;
		private CountryPicker.Builder _builder;
		private CountryPicker _picker;
		private Country _country;
		private FirebaseAuth _auth;
		private FacebookProfileEventListener _profileEventListener;

		private ICallbackManager _callbackManager;
		private LoginResult _loginResult;

		//shared preference
		private readonly ISharedPreferences _preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

		private string _userId;
		private LoginMethodEnums _loginMethod = LoginMethodEnums.PhoneAuth;

		private AlertDialogHelper _dialogHelper;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			_profileEventListener = new FacebookProfileEventListener();
			_profileEventListener.OnProfileChanged += ProfileTracker_OnProfileChanged;
			_profileEventListener.StartTracking();

			SetContentView(Resource.Layout.onboarding_layout);
			GetWidgets();

			_helper = new CookieBarHelper(this);
			_dialogHelper = new AlertDialogHelper(this);
			_auth = AppDataHelper.GetFirebaseAuth();
			_callbackManager = CallbackManagerFactory.Create();
			LoginManager.Instance.RegisterCallback(_callbackManager, this);
		}

		private void RequestLocationPermissions()
		{
			if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted &&
				ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
			{
				RequestPermissions(new[]
				{
					Manifest.Permission.AccessCoarseLocation,
					Manifest.Permission.AccessFineLocation
				}, RequestPermission);
			}
		}

		private void GetWidgets()
		{
			_mGoogleFab = FindViewById<FloatingActionButton>(Resource.Id.fab_google);
			_mGoogleFab.Click += MGoogleFab_Click;
			_mFacebookFab = FindViewById<FloatingActionButton>(Resource.Id.fab_fb);
			_mFacebookFab.Click += MFacebookFab_Click;

			_mRelativeLayout = FindViewById<RelativeLayout>(Resource.Id.onboard_root);
			_mRelativeLayout.RequestFocus();

			_mLinearLayout = FindViewById<LinearLayout>(Resource.Id.mLinear_view_2);

			_mEditText = FindViewById<EditText>(Resource.Id.user_phone_edittext2);
			_mEditText.SetCursorVisible(false);
			_mEditText.Click += MEditText_Click;
			_mEditText.FocusChange += MEditText_FocusChange;
			_mLinearLayout.Click += MLinearLayout_Click;

			//country code tools
			_countryFlagImg = FindViewById<CircleImageView>(Resource.Id.country_flag_img_2);
			_builder = new CountryPicker.Builder().With(this).SortBy(CountryPicker.SortByName);
			_picker = _builder.Build();
			_country = _picker.CountryFromSIM;
			_countryFlagImg.SetBackgroundResource(_country.Flag);
		}

		private void MFacebookFab_Click(object sender, EventArgs e)
		{
			LoginManager.Instance.LogInWithReadPermissions(this, new List<string> { "public_profile", "email" });
			_loginMethod = LoginMethodEnums.FacebookAuth;
		}

		private void MGoogleFab_Click(object sender, EventArgs e)
		{
			_helper.ShowCookieBar("Info", "google login coming soon");
		}

		private void MEditText_FocusChange(object sender, View.FocusChangeEventArgs e)
		{
			if (e.HasFocus)
			{
				GetSharedIntent();
			}
		}

		private void MEditText_Click(object sender, EventArgs e)
		{
			GetSharedIntent();
		}

		private void MLinearLayout_Click(object sender, EventArgs e)
		{
			GetSharedIntent();
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			_callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
		}

		private void GetSharedIntent()
		{
			Intent sharedIntent = new Intent(this, typeof(GetStartedActivity));
			Android.Support.V4.Util.Pair p1 = Android.Support.V4.Util.Pair.Create(_countryFlagImg, "cc_trans");
			Android.Support.V4.Util.Pair p2 = Android.Support.V4.Util.Pair.Create(_mEditText, "edittext_trans");

			ActivityOptionsCompat activityOptions = ActivityOptionsCompat.MakeSceneTransitionAnimation(this, p1, p2);
			StartActivity(sharedIntent, activityOptions.ToBundle());
		}

		protected override void OnResume()
		{
			base.OnResume();
			RequestLocationPermissions();
		}

		public void OnCancel()
		{
			_helper.ShowCookieBar("Info", "facebook login canceled");
		}

		public void OnError(FacebookException error)
		{
			_helper.ShowCookieBar("Facebook Error", error.Message);
		}

		public void OnSuccess(Java.Lang.Object result)
		{
			if (!_usingFirebase)
			{
				_dialogHelper.ShowDialog();
				_usingFirebase = true;
				_loginResult = result as LoginResult;

				var credentials = FacebookAuthProvider.GetCredential(_loginResult.AccessToken.Token);
				_auth.SignInWithCredential(credentials)
					.AddOnSuccessListener(this, this)
					.AddOnFailureListener(this, this);
			}
			else
			{
				_usingFirebase = false;
				_userId = _auth.CurrentUser.Uid;
				CheckIfUserExists();
			}
		}

		public void OnFailure(Java.Lang.Exception e)
		{
			_dialogHelper.CloseDialog();
			_helper.ShowCookieBar("Error", e.Message);
		}

		private void CheckIfUserExists()
		{
			DatabaseReference userRef = AppDataHelper.GetDatabase().GetReference("users");
			userRef.OrderByKey().EqualTo(_userId).AddListenerForSingleValueEvent(this);
		}

		private void SaveToSharedPreference(UserData userData)
		{
			ISharedPreferencesEditor editor = _preferences.Edit();

			editor.PutString("profile_id", userData.ProfileId);
			editor.PutString("email", userData.Email);
			editor.PutString("firstname", userData.FirstName);
			editor.PutString("lastname", userData.LastName);
			editor.PutString("phone", userData.Phone);
			editor.PutInt("logintype", userData.Logintype);
			editor.PutBoolean("isLinked", userData.IsLinked);

			editor.Apply();
		}

		public void OnCancelled(DatabaseError error)
		{
			_dialogHelper.CloseDialog();
			_helper.ShowCookieBar("Error", error.Message);
		}

		public void OnDataChange(DataSnapshot snapshot)
		{
			_dialogHelper.CloseDialog();

			if (snapshot.Value != null)
			{
				DataSnapshot child = snapshot.Child(_userId);

				UserData userData = new UserData
				{
					Email = child?.Child("email")?.Value?.ToString(),
					Phone = child?.Child("phone")?.Value?.ToString(),
					FirstName = child?.Child("firstname")?.Value?.ToString(),
					LastName = child?.Child("lastname")?.Value?.ToString(),
					Logintype = (int)_loginMethod,
					ProfileId = child?.Child("profile_id")?.Value?.ToString(),
					IsLinked = (bool)child?.Child("isLinkedWithAuth")?.Value
				};

				SaveToSharedPreference(userData);

				Intent intent = new Intent(this, typeof(MainActivity));
				intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
				StartActivity(intent);

				OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
				Finish();
			}
			else
			{
				Snackbar.Make(_mRelativeLayout, "Your Facebook is not linked to any Cab 360 account. Please sign up first.", Snackbar.LengthIndefinite)
					.SetAction("OK", delegate
					{
						_auth.SignOut();
						LoginManager.Instance.LogOut();
					})
					.Show();
			}
		}

		protected override void OnStart()
		{
			base.OnStart();
			RequestLocationPermissions();
		}

		private void ProfileTracker_OnProfileChanged(object sender, OnProfileChangedEventArgs e)
		{
			if (e.mProfile != null && _auth?.CurrentUser?.Uid != null)
			{
				FirebaseUser user = AppDataHelper.GetCurrentUser();

				HashMap userMap = new HashMap();
				userMap.Put("profile_id", e.mProfile.Id);
				userMap.Put("email", user.Email);
				userMap.Put("phone", user.PhoneNumber);
				userMap.Put("firstname", e.mProfile.FirstName);
				userMap.Put("lastname", e.mProfile.LastName);
				userMap.Put("isLinkedWithAuth", true);
				userMap.Put("timestamp", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

				DatabaseReference userReference = AppDataHelper.GetDatabase().GetReference("users/" + user.Uid);
				userReference.SetValue(userMap);
				userReference.KeepSynced(true);
			}
		}

		public void OnCompleted(JSONObject @object, GraphResponse response)
		{
			try
			{
				string fbId = response.JSONObject.GetString("id");

				ISharedPreferencesEditor editor = _preferences.Edit();
				editor.PutString("profile_id", fbId);
				editor.Apply();

				var intent = new Intent(this, typeof(MainActivity));
				intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
				StartActivity(intent);
				Finish();
			}
			catch (JSONException e)
			{
				e.PrintStackTrace();
			}
		}
	}
}