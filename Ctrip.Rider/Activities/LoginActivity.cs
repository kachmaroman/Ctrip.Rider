using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Ctrip.Rider.Dtos;
using Ctrip.Rider.EventListeners;
using Ctrip.Rider.Helpers;
using Firebase;
using Firebase.Auth;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "@string/app_name", Theme = "@style/CtripTheme", MainLauncher = true)]
	public class LoginActivity : AppCompatActivity
	{
		private TextInputLayout _emailText;
		private TextInputLayout _passwordText;
		private TextView _clickToRegisterText;
		private Button _loginButton;
		private CoordinatorLayout _rootView;
		private FirebaseAuth _firebaseAuth;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.login);

			_emailText = FindViewById<TextInputLayout>(Resource.Id.emailText);
			_passwordText = FindViewById<TextInputLayout>(Resource.Id.passwordText);
			_rootView = FindViewById<CoordinatorLayout>(Resource.Id.rootView);
			_loginButton = FindViewById<Button>(Resource.Id.loginButton);
			_clickToRegisterText = FindViewById<TextView>(Resource.Id.clickToRegisterText);

			_loginButton.Click += LoginButton_Click;
			_clickToRegisterText.Click += ClickToRegisterText_Click;

			InitializeFirebase();
		}

		private void ClickToRegisterText_Click(object sender, EventArgs e)
		{
			StartActivity(typeof(RegistrationActivity));
		}

		private void LoginButton_Click(object sender, EventArgs e)
		{
			UserLoginDto user = new UserLoginDto
			{
				Email = _emailText.EditText.Text,
				Password = _passwordText.EditText.Text
			};

			ValidationResult validationResult = Validator.Validate(user);

			if (!validationResult.IsValid)
			{
				Snackbar.Make(_rootView, validationResult.ErorMessage, Snackbar.LengthShort).Show();
				return;
			}

			TaskCompletionListener taskCompletionListener = new TaskCompletionListener();
			taskCompletionListener.Success += TaskCompletionListener_Success;
			taskCompletionListener.Failure += TaskCompletionListener_Failure;

			_firebaseAuth.SignInWithEmailAndPassword(user.Email, user.Password)
				.AddOnSuccessListener(taskCompletionListener)
				.AddOnFailureListener(taskCompletionListener);
		}

		private void TaskCompletionListener_Failure(object sender, EventArgs e)
		{
			Snackbar.Make(_rootView, "Login Failed", Snackbar.LengthShort).Show();
		}

		private void TaskCompletionListener_Success(object sender, EventArgs e)
		{
			StartActivity(typeof(MainActivity));
		}

		private void InitializeFirebase()
		{
			FirebaseApp firebaseApp = FirebaseApp.InitializeApp(this);

			if (firebaseApp == null)
			{
				FirebaseApp.InitializeApp(this, FirebaseBuilder.BuildOptions());
			}

			_firebaseAuth = FirebaseAuth.Instance;
		}

	}
}