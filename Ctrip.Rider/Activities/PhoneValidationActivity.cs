using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Com.Goodiebag.Pinview;
using Firebase.Auth;
using Firebase.Database;
using Plugin.Connectivity;
using System;
using Android.Support.Design.Widget;
using Ctrip.Rider.Constants;
using Ctrip.Rider.DataModels;
using Ctrip.Rider.Fragments;
using Ctrip.Rider.Helpers;
using static Com.Goodiebag.Pinview.Pinview;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "Enter code", Theme = "@style/AppTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
	public class PhoneValidationActivity : AppCompatActivity, IPinViewEventListener, IOnCompleteListener, IValueEventListener, IOnFailureListener
	{
		//Views
		private Android.Support.V7.Widget.Toolbar _veriToolbar;
		private RelativeLayout _rootView;
		public Pinview codePinView;
		private TextView _enterCodeTv;
		private TextView _timerTv;
		private Button _nextButton;

		private CookieBarHelper _helper;

		//dialogs
		private Android.Support.V7.App.AlertDialog _alertDialog;
		private Android.Support.V7.App.AlertDialog.Builder _builder;

		internal static PhoneValidationActivity Instance { get; set; }

		public string verificationId;
		private string _intFormat, _userId;

		//shared preference
		readonly ISharedPreferences _preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.phone_validation_layout);

			Instance = this;
			_intFormat = AppDataHelper.GetIntFormat();
			_helper = new CookieBarHelper(this);

			InitControls();
		}

		private void InitControls()
		{
			_veriToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.prim_toolbar1);

			if (_veriToolbar != null)
			{
				SetSupportActionBar(_veriToolbar);
			}

			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
			SupportActionBar.SetHomeButtonEnabled(true);

			//Views
			_rootView = FindViewById<RelativeLayout>(Resource.Id.phone_val_root);
			codePinView = FindViewById<Pinview>(Resource.Id.phone_pinView);
			codePinView.SetPinViewEventListener(this);
			_enterCodeTv = FindViewById<TextView>(Resource.Id.enter_code_tv);

			_timerTv = FindViewById<TextView>(Resource.Id.timer_tv);
			_timerTv.Click += (s2, e2) =>
			{
				//
			};

			_nextButton = (Button)FindViewById(Resource.Id.prim_btn1);
			_nextButton.Click += NextButton_Click;

			string first = "Text message sent to ";

			SpannableString str = new SpannableString(first + _intFormat);
			str.SetSpan(new StyleSpan(TypefaceStyle.Bold), first.Length, first.Length + _intFormat.Length, SpanTypes.ExclusiveExclusive);

			_enterCodeTv.TextFormatted = str;

			AppDataHelper.SendVerificationCode(_intFormat, Instance);
		}

		private void NextButton_Click(object sender, EventArgs e)
		{
			ShowProgressDialog();

			string otpCode = codePinView.Value;

			if (!CrossConnectivity.Current.IsConnected)
			{
				Android.Support.V4.App.DialogFragment dialogFragment = new NoNetworkFragment();
				dialogFragment.Show(SupportFragmentManager, "no network");
			}
			else
			{
				VerifyCode(otpCode);
			}
		}

		public override bool OnSupportNavigateUp()
		{
			Finish();
			return true;
		}

		public void OnDataEntered(Pinview otpView, bool textFromUSer)
		{
			_nextButton.Enabled = codePinView.Value.Length == 6;
		}

		public void VerifyCode(string otpCode)
		{
			if (string.IsNullOrEmpty(verificationId))
			{
				CloseProgressDialog();
				_helper.ShowCookieBar("Error", "Invalid code");

				return;
			}

			PhoneAuthCredential credentials = PhoneAuthProvider.GetCredential(verificationId, otpCode);
			InitializeCredentials(credentials);
		}

		private void InitializeCredentials(PhoneAuthCredential credentials)
		{
			AppDataHelper.GetFirebaseAuth()
				.SignInWithCredential(credentials)
				.AddOnCompleteListener(this)
				.AddOnFailureListener(this);
		}

		public void OnComplete(Task task)
		{
			FirebaseUser user = AppDataHelper.GetCurrentUser();

			if (user == null)
			{
				return;
			}

			_userId = user.Uid;
			CheckIfUserExists(_userId);
		}

		public void OnFailure(Java.Lang.Exception e)
		{
			CloseProgressDialog();
			_helper.ShowCookieBar("Error", "Invalid code");
		}

		private void CheckIfUserExists(string userId)
		{
			DatabaseReference userRef = AppDataHelper.GetDatabase().GetReference("users");
			userRef.OrderByKey().EqualTo(userId).AddListenerForSingleValueEvent(this);
		}

		public void OnCancelled(DatabaseError error)
		{

		}

		public void OnDataChange(DataSnapshot snapshot)
		{
			if (snapshot.Value != null)
			{
				DataSnapshot child = snapshot.Child(_userId);

				UserData userData = new UserData
				{
					Email = child?.Child("email").Value.ToString(),
					Phone = child?.Child("phone").Value.ToString(),
					FirstName = child?.Child("firstname").Value.ToString(),
					LastName = child?.Child("lastname").Value.ToString(),
					Logintype = (int)LoginMethodEnums.PhoneAuth,
					IsLinked = (bool)child?.Child("isLinkedWithAuth").Value
				};

				verificationId = string.Empty;

				SaveToSharedPreference(userData);

				CloseProgressDialog();

				Intent intent = new Intent(this, typeof(MainActivity));
				intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
				StartActivity(intent);

				Finish();
			}
			else
			{
				StartActivity(new Intent(this, typeof(ProfileActivity)));
				OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
			}
		}

		private void SaveToSharedPreference(UserData userData)
		{
			ISharedPreferencesEditor editor = _preferences.Edit();

			editor.PutString("email", userData.Email);
			editor.PutString("firstname", userData.FirstName);
			editor.PutString("lastname", userData.LastName);
			editor.PutString("phone", userData.Phone);
			editor.PutInt("logintype", userData.Logintype);
			editor.PutBoolean("isLinked", userData.IsLinked);

			editor.Apply();
		}

		public void ShowProgressDialog()
		{
			_builder = new Android.Support.V7.App.AlertDialog.Builder(this);
			_builder.SetView(Resource.Layout.progress_dialog_layout);
			_builder.SetCancelable(false);
			_alertDialog = _builder.Show();
		}

		public void CloseProgressDialog()
		{
			if (_alertDialog == null)
			{
				return;
			}

			_alertDialog.Dismiss();
			_alertDialog = null;
			_builder = null;
		}
	}

}