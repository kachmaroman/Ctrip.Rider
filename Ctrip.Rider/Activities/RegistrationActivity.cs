using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using Java.Util;
using Ctrip.Rider.EventListeners;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "@string/app_name", Theme = "@style/CtripTheme", MainLauncher = false)]
	public class RegistrationActivity : AppCompatActivity
	{
        TextInputLayout fullNameText;
        TextInputLayout phoneText;
        TextInputLayout emailText;
        TextInputLayout passwordText;
        Button registerButton;
        CoordinatorLayout rootView;
        TextView clickToLoginText;

        FirebaseAuth mAuth;
        FirebaseDatabase database;
        TaskCompletionListener TaskCompletionListener = new TaskCompletionListener();
        string fullname, phone, email, password;

        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor editor;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);

            InitializeFirebase();
            mAuth = FirebaseAuth.Instance;
            ConnectControl();
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

            database = FirebaseDatabase.GetInstance(app);
        }

        void ConnectControl()
        {
            fullNameText = FindViewById<TextInputLayout>(Resource.Id.fullNameText);
            phoneText = FindViewById<TextInputLayout>(Resource.Id.phoneText);
            emailText = FindViewById<TextInputLayout>(Resource.Id.emailText);
            passwordText = FindViewById<TextInputLayout>(Resource.Id.passwordText);
            rootView = FindViewById<CoordinatorLayout>(Resource.Id.rootView);
            registerButton = FindViewById<Button>(Resource.Id.registerButton);
            clickToLoginText = FindViewById<TextView>(Resource.Id.clickToLogin);

            clickToLoginText.Click += ClickToLoginText_Click;
            registerButton.Click += RegisterButton_Click;
        }

        private void ClickToLoginText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            fullname = fullNameText.EditText.Text;
            phone = phoneText.EditText.Text;
            email = emailText.EditText.Text;
            password = passwordText.EditText.Text;

            if (fullname.Length < 3)
            {
                Snackbar.Make(rootView, "Please enter a valid name", Snackbar.LengthShort).Show();
                return;
            }
            else if (phone.Length < 9)
            {
                Snackbar.Make(rootView, "Please enter a valid phone number", Snackbar.LengthShort).Show();
                return;
            }
            else if (!email.Contains("@"))
            {
                Snackbar.Make(rootView, "Please enter a valid email", Snackbar.LengthShort).Show();
                return;
            }
            else if (password.Length < 8)
            {
                Snackbar.Make(rootView, "Please enter a password upto 8 characters", Snackbar.LengthShort).Show();
                return;
            }

            RegisterUser(fullname, phone, email, password);
        }

        void RegisterUser(string name, string phone, string email, string password)
        {
            TaskCompletionListener.Success += TaskCompletionListener_Success;
            TaskCompletionListener.Failure += TaskCompletionListener_Failure;

            mAuth.CreateUserWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, TaskCompletionListener)
                .AddOnFailureListener(this, TaskCompletionListener);

        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User Registration failed", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            Snackbar.Make(rootView, "User Registration was Successful", Snackbar.LengthShort).Show();

            HashMap userMap = new HashMap();
            userMap.Put("email", email);
            userMap.Put("phone", phone);
            userMap.Put("fullname", fullname);

            DatabaseReference userReference = database.GetReference("users/" + mAuth.CurrentUser.Uid);
            userReference.SetValue(userMap);
        }

        void SaveToSharedPreference()
        {
            editor = preferences.Edit();

            editor.PutString("email", email);
            editor.PutString("fullname", fullname);
            editor.PutString("phone", phone);

            editor.Apply();
        }

        void RetriveData()
        {
            string email = preferences.GetString("email", "");
        }

    }
}