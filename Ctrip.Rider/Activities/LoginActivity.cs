using System;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Ctrip.Rider.EventListeners;
using Firebase;
using Firebase.Auth;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "@string/app_name", Theme = "@style/CtripTheme", MainLauncher = true)]
	public class LoginActivity : AppCompatActivity
	{
		TextInputLayout emailText;
		TextInputLayout passwordText;
		TextView clickToRegisterText;
		Button loginButton;
		CoordinatorLayout rootView;
		FirebaseAuth firebaseAuth;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your application here

			SetContentView(Resource.Layout.login);

			emailText = (TextInputLayout)FindViewById(Resource.Id.emailText);
			passwordText = (TextInputLayout)FindViewById(Resource.Id.passwordText);
			rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
			loginButton = (Button)FindViewById(Resource.Id.loginButton);
			clickToRegisterText = (TextView)FindViewById(Resource.Id.clickToRegisterText);

			clickToRegisterText.Click += ClickToRegisterText_Click;
			loginButton.Click += LoginButton_Click;

			InitializeFirebase();
		}

		private void ClickToRegisterText_Click(object sender, EventArgs e)
		{
			StartActivity(typeof(RegistrationActivity));
		}

		private void LoginButton_Click(object sender, EventArgs e)
		{
			string email, password;

			email = emailText.EditText.Text;
			password = passwordText.EditText.Text;

			if (!email.Contains("@"))
			{
				Snackbar.Make(rootView, "Please provide a valid email", Snackbar.LengthShort).Show();
				return;
			}
			else if (password.Length < 8)
			{
				Snackbar.Make(rootView, "Please provide a valid password", Snackbar.LengthShort).Show();
				return;
			}

			TaskCompletionListener taskCompletionListener = new TaskCompletionListener();
			taskCompletionListener.Success += TaskCompletionListener_Success;
			taskCompletionListener.Failure += TaskCompletionListener_Failure;

			firebaseAuth.SignInWithEmailAndPassword(email, password)
				.AddOnSuccessListener(taskCompletionListener)
				.AddOnFailureListener(taskCompletionListener);
		}

		private void TaskCompletionListener_Failure(object sender, EventArgs e)
		{
			Snackbar.Make(rootView, "Login Failed", Snackbar.LengthShort).Show();
		}

		private void TaskCompletionListener_Success(object sender, EventArgs e)
		{
			StartActivity(typeof(MainActivity));
		}

		private void InitializeFirebase()
		{
			var app = FirebaseApp.InitializeApp(this);

			if (app == null)
			{
				var options = new FirebaseOptions.Builder()
					.SetApplicationId("ctrip-50eab")
					.SetApiKey("AIzaSyDBk-f9zqpg1uGZYAHUt5kV8xbOxGQiS9w")
					.SetDatabaseUrl("https://ctrip-50eab.firebaseio.com")
					.SetStorageBucket("ctrip-50eab.appspot.com")
					.Build();

				app = FirebaseApp.InitializeApp(this, options);
			}

			firebaseAuth = FirebaseAuth.Instance;
		}

	}
}