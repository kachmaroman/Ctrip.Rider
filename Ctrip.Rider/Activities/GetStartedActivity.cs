using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using Com.Mukesh.CountryPickerLib;
using Com.Mukesh.CountryPickerLib.Listeners;
using Ctrip.Rider.Fragments;
using Ctrip.Rider.Helpers;
using Google.I18n.PhoneNumbers;
using Java.Lang;
using Plugin.Connectivity;
using Refractored.Controls;
using static Android.Views.View;
using Result = Android.App.Result;

namespace Ctrip.Rider.Activities
{
    [Activity(Theme = "@style/AppTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, ScreenOrientation = ScreenOrientation.Portrait, WindowSoftInputMode = SoftInput.AdjustResize)]
    public class GetStartedActivity : AppCompatActivity, ITextWatcher, IOnKeyListener, IOnCountryPickerListener
    {
        //widgets
        private Button _primaryButton;
        private EditText _userPhoneText;
        private CircleImageView _countryFlagImg;
        private LinearLayout _ccLayout;
        private TextView _cctv;
        private Android.Support.V7.Widget.Toolbar _mToolbar;
        private CookieBarHelper _helper;

        //country picker
        private CountryPicker.Builder _builder;
        private CountryPicker _picker;
        private Country _country;
        private string _countryCode;

        //shared preference
        private readonly ISharedPreferences _preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

        internal static GetStartedActivity Instance { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.getting_started_layout);
            Instance = this;

            _helper = new CookieBarHelper(this);
            InitControls();
        }

        public void AfterTextChanged(IEditable s)
        {
	        ProcessButtonByTextLength();
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            
        }

        public bool OnKey(View v, [GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
	        if (e.Action == KeyEventActions.Up)
            {
                ProcessButtonByTextLength();
            }

            return false;
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            
        }

        private void ProcessButtonByTextLength()
        {
	        string inputText = _userPhoneText.Text;

	        if (inputText.Length >= 7)
	        {
		        _primaryButton.Enabled = true;
		        _primaryButton.SetTextColor(Color.White);
	        }
	        else
	        {
		        _primaryButton.Enabled = false;
	        }
        }

        private void InitControls()
        {
	        _mToolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.getting_started_toolbar);

	        if (_mToolbar != null)
	        {
		        SetSupportActionBar(_mToolbar);
            }

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.Title = string.Empty;   

            _ccLayout = (LinearLayout)FindViewById(Resource.Id.cc_layout);
            _ccLayout.Click += (s3, e3) =>
            {
                _picker.ShowBottomSheet(this);
            };
            _cctv = (TextView)FindViewById(Resource.Id.cc_textview);

            //country code tools
            _countryFlagImg = (CircleImageView)FindViewById(Resource.Id.country_flag_img);
            _countryFlagImg.RequestFocus();
            _builder = new CountryPicker.Builder().With(this).Listener(this).SortBy(CountryPicker.SortByName);
            _picker = _builder.Build();
            _country = _picker.CountryFromSIM;
            _countryCode = _country.Code;
            _countryFlagImg.SetBackgroundResource(_country.Flag);
            _cctv.Text = _country.DialCode;
            
            _userPhoneText = (EditText)FindViewById(Resource.Id.user_phone_edittext);
            _userPhoneText.AddTextChangedListener(this);
            _userPhoneText.SetOnKeyListener(this);

            _primaryButton = (Button)FindViewById(Resource.Id.primary_btn);
            _primaryButton.Click += (s1, e1) =>
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    Android.Support.V4.App.DialogFragment dialogFragment = new NoNetworkFragment();
                    dialogFragment.Show(SupportFragmentManager, "no network");

                }
                else
                {
	                ValidatePhoneNumberAndCode();
                }
            };
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void ValidatePhoneNumberAndCode()
        {
	        if (string.IsNullOrEmpty(_countryCode))
            {
                return;
            }

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.Instance;
            Phonenumber.PhoneNumber phoneProto = null;

            try
            {
                phoneProto = phoneUtil.Parse(_userPhoneText.Text, _countryCode);
            }
            catch(NumberParseException npe)
            {
                Toast.MakeText(this, $"error: {npe.Message}", ToastLength.Short).Show();
            }

            bool isValid = phoneUtil.IsValidNumber(phoneProto);

            if (isValid)
            {
	            string intFormat = phoneUtil.Format(phoneProto, PhoneNumberUtil.PhoneNumberFormat.International);

                SaveToSharedPreference(intFormat, phoneProto?.ToString());

                Intent myintent = new Intent(this, typeof(PhoneValidationActivity));

                StartActivity(myintent);

                OverridePendingTransition(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out);
            }
            else
            {
                _helper.ShowCookieBar("Error", "Invalid phone number");
            }
        }

        private void SaveToSharedPreference(string intFormat, string phoneProto)
        {
	        ISharedPreferencesEditor editor = _preferences.Edit();

            editor.PutString("int_format", intFormat);
            editor.PutString("phoneProto", phoneProto);

            editor.Apply();
        }

        public void OnSelectCountry(Country country)
        {
            _countryCode = country.Code;
            _countryFlagImg.SetBackgroundResource(country.Flag);
            _cctv.Text = country.DialCode;
            _userPhoneText.RequestFocus();
        }

        public override bool OnSupportNavigateUp()
        {
	        KeyboardHelper.HideKeyboard(this);
            SupportFinishAfterTransition();
            return true;
        }

        public override void OnBackPressed()
        {
	        KeyboardHelper.HideKeyboard(this);
            SupportFinishAfterTransition();
            base.OnBackPressed();
            Finish();
        }
    }
}