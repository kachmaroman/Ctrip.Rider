using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using ActionBar = Android.Support.V7.App.ActionBar;

namespace Ctrip.Rider
{
    [Activity(Label = "@string/app_name", Theme = "@style/CtripTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        private Toolbar _mainToolbar;
        private DrawerLayout _drawerLayout;

	    protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            ConnectControls();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ConnectControls()
        {
	        _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawerLayout);
            _mainToolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar);
            SetSupportActionBar(_mainToolbar);
            SupportActionBar.Title = string.Empty;
            ActionBar actionBar = SupportActionBar;
            actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
            actionBar.SetDisplayHomeAsUpEnabled(true);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
	        switch (item.ItemId)
	        {
                case Android.Resource.Id.Home:
	                _drawerLayout.OpenDrawer((int)GravityFlags.Left);
	                return true;
                default:
	                return base.OnOptionsItemSelected(item);
            }
        }
    }
}