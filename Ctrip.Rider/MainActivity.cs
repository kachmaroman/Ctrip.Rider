using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Ctrip.Rider.Helpers;
using ActionBar = Android.Support.V7.App.ActionBar;

namespace Ctrip.Rider
{
    [Activity(Label = "@string/app_name", Theme = "@style/CtripTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
        private Toolbar _mainToolbar;
        private DrawerLayout _drawerLayout;
        private GoogleMap _googleMap;

        private readonly string[] _permissionGroupLocation =
	        {Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation};

        private const int RequestLocationId = 0;

        private LocationRequest _mLocationRequest;
        private FusedLocationProviderClient _locationProviderClient;
        private Location _mLastLocation;
        private LocationCallbackHelper _mLocationCalback;

        private static readonly int UpdateInterval = 5; //5 SECONDS
        private static readonly int FastestInterval = 5;
        private static readonly int Displacement = 3; //meters
        private static readonly int Zoom = 15;

	    protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            ConnectControls();

            SupportMapFragment mapFragment = (SupportMapFragment)SupportFragmentManager.FindFragmentById(Resource.Id.map);
            mapFragment.GetMapAsync(this);

            CheckLocationPermissions();
            CreateLocationRequest();
	        await GetCurrentLocationAsync();
	        await StartLocationUpdatesAsync();
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

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
	        Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

	        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnMapReady(GoogleMap googleMap)
        {
	        _googleMap = googleMap;
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

        private bool CheckLocationPermissions()
        {
	        bool permissionsGranted = _permissionGroupLocation.All(x => ContextCompat.CheckSelfPermission(this, x) == Permission.Granted);

	        if (!permissionsGranted)
	        {
		        RequestPermissions(_permissionGroupLocation, RequestLocationId);
	        }

	        return permissionsGranted;
        }

        private void CreateLocationRequest()
        {
	        _mLocationRequest = new LocationRequest();
	        _mLocationRequest.SetInterval(UpdateInterval);
	        _mLocationRequest.SetFastestInterval(FastestInterval);
	        _mLocationRequest.SetSmallestDisplacement(Displacement);
            _mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);

            _locationProviderClient = LocationServices.GetFusedLocationProviderClient(this);

            _mLocationCalback = new LocationCallbackHelper();
            _mLocationCalback.CurrentLocation += CurrentLocationCallback;
        }

        private async Task GetCurrentLocationAsync()
        {
	        if (!CheckLocationPermissions())
	        {
		        return;
	        }

	        _mLastLocation = await _locationProviderClient.GetLastLocationAsync();

	        if (_mLastLocation != null)
	        {
		        LatLng currentLocation = new LatLng(_mLastLocation.Latitude, _mLastLocation.Longitude);
		        _googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(currentLocation, Zoom));
	        }
        }

        private void CurrentLocationCallback(object sender, OnLocationCapturedEventArgs e)
        {
	        _mLastLocation = e.Location;
	        LatLng currentPosition = new LatLng(_mLastLocation.Latitude, _mLastLocation.Longitude);
	        _googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(currentPosition, 12));
        }

        private async Task StartLocationUpdatesAsync()
        {
	        if (CheckLocationPermissions())
	        {
		        await _locationProviderClient.RequestLocationUpdatesAsync(_mLocationRequest, _mLocationCalback, null);
	        }
        }

        private async Task StopLocationUpdatesAsync()
        {
	        if (_locationProviderClient != null && _mLocationCalback != null)
	        {
		        await _locationProviderClient.RemoveLocationUpdatesAsync(_mLocationCalback);
	        }
        }
    }
}