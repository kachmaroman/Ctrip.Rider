using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Firebase.Database;
using Java.Lang;
using Java.Util;
using System;
using System.Globalization;
using Ctrip.Rider.Helpers;
using static Android.Views.View;

namespace Ctrip.Rider.Activities
{
    [Activity(Label = "@string/txtProfile", Theme = "@style/AppTheme",ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.SmallestScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class ProfileActivity : AppCompatActivity, ITextWatcher, IOnKeyListener
    {
        private Button _profileNextBtn;
        private TextInputEditText _emailEditText;
        private TextInputEditText _firstNameEditText;
        private TextInputEditText _lastNameEditText;

        private FirebaseAuth _mAuth;
        private FirebaseDatabase _database;
        private string _userPhone;

        //shared preference
        readonly ISharedPreferences _preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.profile_layout);

            _mAuth = AppDataHelper.GetFirebaseAuth();
            _database = AppDataHelper.GetDatabase();
            _userPhone = AppDataHelper.GetFirebaseAuth().CurrentUser.PhoneNumber;

            InitControls();

            Android.Support.V7.Widget.Toolbar profileToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.profile_toolbar);

            if (profileToolbar != null)
            {
	            SetSupportActionBar(profileToolbar);
            }

	        SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        public void AfterTextChanged(IEditable s)
        {
            CheckIfEmpty();
        }

        private void CheckIfEmpty()
        {
            var email = _emailEditText.Text;
            var fname = _firstNameEditText.Text;
            var lname = _lastNameEditText.Text;

            _profileNextBtn.Enabled = Android.Util.Patterns.EmailAddress.Matcher(email).Matches() && fname.Length >= 3 && lname.Length >= 3;
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            
        }

        public bool OnKey(View v, [GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
	        if (e.Action == KeyEventActions.Up)
            {
                CheckIfEmpty();
            }

            return false;
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            
        }

        private void InitControls()
        {
            //Terms and conditions
            TextView termsText = FindViewById<TextView>(Resource.Id.terms_tv);

            string first = "By signing up you agree to our ";
            string last = "Terms and Conditions";

            SpannableString str = new SpannableString(first + last);
            str.SetSpan(new StyleSpan(TypefaceStyle.Bold), first.Length, first.Length + last.Length, SpanTypes.ExclusiveExclusive);
            termsText.TextFormatted = str;

            //email
            _emailEditText = FindViewById<TextInputEditText>(Resource.Id.email_edittext);
            _emailEditText.SetOnKeyListener(this);
            _emailEditText.AddTextChangedListener(this);

            //firstname
            _firstNameEditText = FindViewById<TextInputEditText>(Resource.Id.fname_edittext);
            _firstNameEditText.SetOnKeyListener(this);
            _firstNameEditText.AddTextChangedListener(this);

            //lastname
            _lastNameEditText = FindViewById<TextInputEditText>(Resource.Id.lname_edittext);
            _lastNameEditText.SetOnKeyListener(this);
            _lastNameEditText.AddTextChangedListener(this);

            _profileNextBtn = FindViewById<Button>(Resource.Id.profile_prim_btn);
            _profileNextBtn.Click += (s1, e1) =>
            {
                string email = _emailEditText.Text.Trim();
                string firstname = _firstNameEditText.Text.Trim();
                string lastname = _lastNameEditText.Text.Trim();

                HashMap userMap = new HashMap();
                userMap.Put("email", email);
                userMap.Put("phone", _userPhone);
                userMap.Put("firstname", firstname);
                userMap.Put("lastname", lastname);
                userMap.Put("isLinkedWithAuth", false);
                userMap.Put("timestamp", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

                DatabaseReference userReference = _database.GetReference("users/" + _mAuth.CurrentUser.Uid);
                userReference.SetValue(userMap);
                userReference.KeepSynced(true);

                SaveToSharedPreference(email, _userPhone, firstname, lastname, false);
                KeyboardHelper.HideKeyboard(this);

                Intent intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
                StartActivity(intent);

                OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
                Finish();
            };
        }

        private void SaveToSharedPreference(string email, string phone, string firstname, string lastname, bool isLinked)
        {
	        ISharedPreferencesEditor editor = _preferences.Edit();
            editor.PutString("email", email);
            editor.PutString("firstname", firstname);
            editor.PutString("lastname", lastname);
            editor.PutString("phone", phone);
            editor.PutBoolean("isLinked", isLinked);

            editor.Apply();
        }

        public override bool OnSupportNavigateUp()
        {
            Finish();
            return true;
        }
    }
}