using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Widget;
using Ctrip.Rider.Dtos;
using Firebase.Auth;
using Firebase.Database;
using Firebase;
using Java.Util;
using Ctrip.Rider.EventListeners;
using Ctrip.Rider.Helpers;

namespace Ctrip.Rider.Activities
{
	[Activity(Label = "@string/app_name", Theme = "@style/CtripTheme", MainLauncher = false)]
	public class RegistrationActivity : AppCompatActivity
	{
        private TextInputLayout _fullNameText;
        private TextInputLayout _phoneText;
        private TextInputLayout _emailText;
        private TextInputLayout _passwordText;
        private Button _registerButton;
        private CoordinatorLayout _rootView;
        private TextView _clickToLoginText;

        private FirebaseAuth _firebaseAuth;
        private FirebaseDatabase _database;
        readonly TaskCompletionListener _taskCompletionListener = new TaskCompletionListener();

        private ISharedPreferencesEditor _editor;

        private readonly UserRegisterDto _user = new UserRegisterDto();
        private readonly ISharedPreferences _preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.register);

            InitializeFirebase();
            _firebaseAuth = FirebaseAuth.Instance;
            ConnectControl();
        }

        private void InitializeFirebase()
        {
	        FirebaseApp firebaseApp = FirebaseApp.InitializeApp(this) ?? FirebaseApp.InitializeApp(this, FirebaseBuilder.BuildOptions());
	        _database = FirebaseDatabase.GetInstance(firebaseApp);
        }

        private void ConnectControl()
        {
	        _fullNameText = FindViewById<TextInputLayout>(Resource.Id.fullNameText);
            _phoneText = FindViewById<TextInputLayout>(Resource.Id.phoneText);
            _emailText = FindViewById<TextInputLayout>(Resource.Id.emailText);
            _passwordText = FindViewById<TextInputLayout>(Resource.Id.passwordText);
            _rootView = FindViewById<CoordinatorLayout>(Resource.Id.rootView);
            _registerButton = FindViewById<Button>(Resource.Id.registerButton);
            _clickToLoginText = FindViewById<TextView>(Resource.Id.clickToLogin);

            _registerButton.Click += RegisterButton_Click;
            _clickToLoginText.Click += ClickToLoginText_Click;
        }

        private void ClickToLoginText_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(LoginActivity));
            Finish();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
	        _user.Fullname = _fullNameText.EditText.Text;
	        _user.Phone = _phoneText.EditText.Text;
	        _user.Email = _emailText.EditText.Text;
	        _user.Password = _passwordText.EditText.Text;

	        ValidationResult validationResult = Validator.Validate(_user);

            if (!validationResult.IsValid)
            {
	            Snackbar.Make(_rootView, validationResult.ErorMessage, Snackbar.LengthShort).Show();
	            return;
            }

            RegisterUser(_user);
        }

        private void RegisterUser(UserRegisterDto user)
        {
            _taskCompletionListener.Success += TaskCompletionListener_Success;
            _taskCompletionListener.Failure += TaskCompletionListener_Failure;

            _firebaseAuth.CreateUserWithEmailAndPassword(user.Email, user.Password)
                .AddOnSuccessListener(this, _taskCompletionListener)
                .AddOnFailureListener(this, _taskCompletionListener);
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            Snackbar.Make(_rootView, "User Registration failed", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Success(object sender, EventArgs e)
        {
            Snackbar.Make(_rootView, "User Registration was Successful", Snackbar.LengthShort).Show();

            HashMap userMap = new HashMap();
            userMap.Put("email", _user.Email);
            userMap.Put("phone", _user.Phone);
            userMap.Put("fullname", _user.Fullname);

            DatabaseReference userReference = _database.GetReference("users/" + _firebaseAuth.CurrentUser.Uid);
            userReference.SetValue(userMap);
        }

        private void SaveToSharedPreference()
        {
            _editor = _preferences.Edit();

            _editor.PutString("email", _user.Email);
            _editor.PutString("fullname", _user.Fullname);
            _editor.PutString("phone", _user.Phone);

            _editor.Apply();
        }
	}
}