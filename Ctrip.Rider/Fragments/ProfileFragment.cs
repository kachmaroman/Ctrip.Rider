using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using Firebase.Auth;
using Org.Json;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using Ctrip.Rider.Activities;
using Ctrip.Rider.Constants;
using Ctrip.Rider.Helpers;
using Firebase.Database;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using static Xamarin.Facebook.GraphRequest;

namespace Ctrip.Rider.Fragments
{
    public class ProfileFragment : Android.Support.V4.App.Fragment, IFacebookCallback, IOnCompleteListener, IGraphJSONObjectCallback
    {
	    private RelativeLayout _profileRoot;
        private Android.Support.V7.Widget.Toolbar _toolbar;
        private LoginButton _fbLoginBtn;
        private ICallbackManager _callbackManager;
        private FirebaseAuth _mAuth;
        private bool _usingFirebase;
        MainActivity _mainActivity;
        private string _email;
        private CircleImageView _profileImg;
        private RelativeLayout _homeRelative, _workRelative;
        //shared preference
        ISharedPreferences preferences = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);
        ISharedPreferencesEditor _editor;

        private Button _logOutBtn;

        //dialogs
        private Android.App.AlertDialog _alertDialog;
        private Android.App.AlertDialog.Builder _builder;

        private TextView _menuBtn;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _mAuth = AppDataHelper.GetFirebaseAuth();
            _mainActivity = MainActivity.Instance;
            _callbackManager = CallbackManagerFactory.Create();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.profile_main, container, false);

            _toolbar = (Android.Support.V7.Widget.Toolbar)view.FindViewById(Resource.Id.custom_toolbar);

            _fbLoginBtn = view.FindViewById<LoginButton>(Resource.Id.fb_btn);
            _fbLoginBtn.SetPermissions(new List<string> { "public_profile", "email" });
            _fbLoginBtn.Fragment = this;
            _fbLoginBtn.RegisterCallback(_callbackManager, this);

            _homeRelative = (RelativeLayout)view.FindViewById(Resource.Id.add_home_rl);
            _workRelative = (RelativeLayout)view.FindViewById(Resource.Id.add_work_rl);
            _homeRelative.Click += HomeRelative_Click; _workRelative.Click += WorkRelative_Click;

            _profileImg = (CircleImageView)view.FindViewById(Resource.Id.profile_ivew);

            string fbId = AppDataHelper.GetFbProfilePic();

            _logOutBtn = (Button)view.FindViewById(Resource.Id.log_out_btn);
            _logOutBtn.Click += LogOutBtn_Click;

            _profileRoot = (RelativeLayout)view.FindViewById(Resource.Id.profile_main_root);

            string email = AppDataHelper.GetEmail();
            bool isLinked = AppDataHelper.IsProviderLinked();

            TextView phone = view.FindViewById<TextView>(Resource.Id.profile_txt2);
            phone.Text = AppDataHelper.GetPhone();

            TextView fullname = view.FindViewById<TextView>(Resource.Id.profile_txt1);
            fullname.Text = AppDataHelper.GetFullName();

            _menuBtn = view.FindViewById<TextView>(Resource.Id.edit_menu);
            _menuBtn.Click += MenuBtn_Click;

            LoginMethodEnums logintype = (LoginMethodEnums)AppDataHelper.GetLogintype();

            switch (logintype)
            {
                case LoginMethodEnums.PhoneAuth:
	                if (isLinked)
                    {
                        _fbLoginBtn.Visibility = ViewStates.Invisible;
                    }
                    break;
                case LoginMethodEnums.FacebookAuth:
	                _mainActivity.RunOnUiThread(() => 
                    {
                        _fbLoginBtn.Visibility = ViewStates.Invisible;
                        SetProfilePic(fbId, _profileImg);
                    });
                    break;
                case LoginMethodEnums.GoogleAuth:
	                _fbLoginBtn.Visibility = ViewStates.Invisible;
                    break;
                default:
                    Toast.MakeText(Application.Context, "No such data", ToastLength.Short).Show();
                    break;
            }

            return view;
        }

        private void WorkRelative_Click(object sender, EventArgs e)
        {
            PlaceTypeRequest(1);
        }

        private void HomeRelative_Click(object sender, EventArgs e)
        {
            PlaceTypeRequest(2);
        }

        private void PlaceTypeRequest(int index)
        {
            
        }

        private void MenuBtn_Click(object sender, EventArgs e)
        {
            PopupMenu popupMenu = new PopupMenu(_mainActivity, _menuBtn, GravityFlags.ClipVertical);
            popupMenu.MenuInflater.Inflate(Resource.Menu.profile_edit_menu, popupMenu.Menu);
            //popupMenu.Inflate(Resource.Menu.package_menu);
            popupMenu.MenuItemClick += (se, ev) =>
            {
                switch (ev.Item.ItemId)
                {
                    case Resource.Id.action_edit_firstname:

                        break;

                    case Resource.Id.action_edit_lastname:

                        break;

                    case Resource.Id.action_edit_email:

                        break;
                }
            };
            popupMenu.Show();
        }

        private async void SetProfilePic(string providerId, CircleImageView imageView)
        {
            try
            {
                await ImageService.Instance
                   .LoadUrl($"https://graph.facebook.com/{providerId}/picture?type=normal")
                   .LoadingPlaceholder("boy_new.png", FFImageLoading.Work.ImageSource.CompiledResource)
                   .Retry(3, 200)
                   .IntoAsync(imageView);
            }
            catch(Exception ex)
            {
                Toast.MakeText(Application.Context, $"Profile Error: {ex.Message}", ToastLength.Short).Show();
            }

        }

        private void LogOutBtn_Click(object sender, System.EventArgs e)
        {
            ShowLogoutDialog();
        }

        private void ShowLogoutDialog()
        {
            _builder = new Android.App.AlertDialog.Builder(_mainActivity);
            _alertDialog = _builder.Create();
            _alertDialog.SetMessage(Resources.GetText(Resource.String.txtLogOutMessage));
            _alertDialog.SetButton(Resources.GetText(Resource.String.txtLogOutYes), (s1, e1) =>
            {
                FirebaseAuth auth = AppDataHelper.GetFirebaseAuth();
                _editor = preferences.Edit();
                LoginManager.Instance.LogOut();
                auth.SignOut();
                _editor.Clear();
                _editor.Commit();

                Intent intent = new Intent(Application.Context, typeof(OnboardingActivity));
                intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.ClearTop | ActivityFlags.NewTask);
                StartActivity(intent);
                _mainActivity.Finish();
            });

            _alertDialog.SetButton2(Resources.GetText(Resource.String.txtLogOutNo), (s2, e2) =>
            {
                _alertDialog.Dismiss();
            });
            _alertDialog.Show();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            _callbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        }

        public void OnCancel()
        {
            
        }

        public void OnError(FacebookException error)
        {
            
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            LoginResult loginResult = result as LoginResult;

            if (loginResult == null)
            {
	            return;
            }

            if (!_usingFirebase)
            {
                _usingFirebase = true;
                AuthCredential authCredential = FacebookAuthProvider.GetCredential(loginResult.AccessToken.Token);
                _mAuth.CurrentUser.LinkWithCredential(authCredential)
	                .AddOnCompleteListener(this);
            }
            else
            {
                _usingFirebase = false;
                SetFacebookData(loginResult);
            }
        }

        public void OnComplete(Task task)
        {
            if (task.IsSuccessful)
            {
                _email = _mAuth.CurrentUser.Email;

            }
            else
            {
                LoginManager.Instance.LogOut();
                Toast.MakeText(Application.Context, task.Exception.Message, ToastLength.Short).Show();
            }
        }

        private void SetFacebookData(LoginResult loginResult)
        {
            GraphRequest graphRequest = NewMeRequest(loginResult.AccessToken, this);
            Bundle parameters = new Bundle();
            parameters.PutString("fields", "id,email,first_name,last_name,picture");
            graphRequest.Parameters = parameters;
            graphRequest.ExecuteAsync();
        }

        public void OnCompleted(JSONObject @object, GraphResponse response)
        {
            try
            {
                string fbid = response.JSONObject.GetString("id");
                string _email = response.JSONObject.GetString("email");
                string firstname = response.JSONObject.GetString("first_name");
                string lastname = response.JSONObject.GetString("last_name");
                
                SetProfilePic(fbid, _profileImg);
            }
            catch (JSONException e)
            {
                e.PrintStackTrace();
            }
        }

        private async void UpdateIsLinkedAsync(bool isLinked)
        {
            DatabaseReference dbref = AppDataHelper.GetDatabase().GetReference("users");
            await dbref.Child(_mAuth.CurrentUser.Uid).Child("isLinkedWithAuth")
                .SetValueAsync(isLinked);

            _editor = preferences.Edit();
            _editor.PutBoolean("isLinked", isLinked);
            _editor.Apply();

            _fbLoginBtn.Visibility = ViewStates.Gone;
        }
    }
}