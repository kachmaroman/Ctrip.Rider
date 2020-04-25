using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using Ctrip.Rider.DataModels;
using Ctrip.Rider.EventListeners;
using Ctrip.Rider.Fragments;
using Ctrip.Rider.Helpers;
using Google.Places;
using ActionBar = Android.Support.V7.App.ActionBar;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using Location = Android.Locations.Location;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Result = Android.App.Result;

namespace Ctrip.Rider
{
    [Activity(Label = "@string/app_name", Theme = "@style/CtripTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, IOnMapReadyCallback
    {
	    //FireBase
	    private readonly UserProfileEventListener _profileEventListener = new UserProfileEventListener();
	    private CreateRequestEventListener _requestEventListener;

		//Views
        private Toolbar _mainToolbar;
        private DrawerLayout _drawerLayout;

		//TextViews
		private TextView _accountTitleText;
        private TextView _pickupLocationText;
        private TextView _destinationText;

		//ImageView
		private ImageView _centerMarker;

        //Layouts
        private RelativeLayout _layoutPickUp;
        private RelativeLayout _layoutDestination;

		//Bottom-sheets
		BottomSheetBehavior _tripDetailsBottomSheetBehavior;

		//Buttons
		private RadioButton _pickupRadio;
		private RadioButton _destitationRadio;
		private Button _favouritePlacesButton;
		private Button _locationSetButton;
		private Button _requestDriverButton;

		//Fragments
		private RequestDriver requestDriverFragment;

		private readonly string[] _permissionGroupLocation =
	        {Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation};

        private const int RequestLocationId = 0;

        private GoogleMap _googleMap;
        private LocationRequest _mLocationRequest;
        private FusedLocationProviderClient _locationProviderClient;
        private Location _mLastLocation;
        private LocationCallbackHelper _mLocationCalback;

        private static readonly int UpdateInterval = 5; //5 SECONDS
        private static readonly int FastestInterval = 5;
        private static readonly int Displacement = 3; //meters
        private static readonly int Zoom = 15;

		//Helpers
		MapFunctionHelper _mapHelper;

		//TripDetails
		private LatLng _pickupLocationLatlng;
		private LatLng _destinationLatLng;
		private string _pickupAddress;
		private string _destinationAddress;

		//Flags
		private int _addressRequest = 1;
		private bool _takeAddressFromSearch = false;

	    //DataModels
	    private NewTripDetails _newTripDetails;


	    private void ConnectControls()
	    {
		    _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawerLayout);
		    _mainToolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar);

		    SetSupportActionBar(_mainToolbar);

		    SupportActionBar.Title = string.Empty;

		    ActionBar actionBar = SupportActionBar;
		    actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
		    actionBar.SetDisplayHomeAsUpEnabled(true);

		    _pickupLocationText = FindViewById<TextView>(Resource.Id.pickupLocationText);
		    _destinationText = FindViewById<TextView>(Resource.Id.destinationText);
		    _layoutPickUp = FindViewById<RelativeLayout>(Resource.Id.layoutPickup);
		    _layoutDestination = FindViewById<RelativeLayout>(Resource.Id.layoutDestination);
		    _layoutPickUp.Click += LayoutPickUp_Click;
		    _layoutDestination.Click += LayoutDestination_Click;

		    _pickupRadio = FindViewById<RadioButton>(Resource.Id.pickupRadio);
		    _destitationRadio = FindViewById<RadioButton>(Resource.Id.destinationRadio);
		    _favouritePlacesButton = FindViewById<Button>(Resource.Id.favouritePlacesButton);
		    _locationSetButton = FindViewById<Button>(Resource.Id.locationsSetButton);
		    _requestDriverButton = FindViewById<Button>(Resource.Id.requestDriverButton);

			_pickupRadio.Click += PickupRadio_Click;
		    _destitationRadio.Click += DestinationRadio_Click;
		    _favouritePlacesButton.Click += FavouritePlacesButoon_Click;
		    _locationSetButton.Click += LocationSetButton_Click;
		    _requestDriverButton.Click += RequestDriverButton_Click;

		    _centerMarker = FindViewById<ImageView>(Resource.Id.centerMarker);

		    FrameLayout tripDetailsView = FindViewById<FrameLayout>(Resource.Id.tripdetails_bottomsheet);
		    _tripDetailsBottomSheetBehavior = BottomSheetBehavior.From(tripDetailsView);
	    }

		#region Overrides

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

			_profileEventListener.Create();

			_accountTitleText = FindViewById<TextView>(Resource.Id.accountTitle);
			_accountTitleText.Text = AppDataHelper.GetFullName();

			InitializePlaces();
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
	        base.OnActivityResult(requestCode, resultCode, data);

	        if (requestCode == 1)
	        {
		        if (resultCode == Result.Ok)
		        {
			        _takeAddressFromSearch = true;
			        _pickupRadio.Checked = false;
			        _destitationRadio.Checked = false;

			        Place place = Autocomplete.GetPlaceFromIntent(data);
			        _pickupLocationText.Text = place.Name;
			        _pickupLocationLatlng = place.LatLng;
			        _pickupAddress = place.Name;
			        _googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
			        _centerMarker.SetColorFilter(Color.DarkGreen);
		        }
	        }

	        if (requestCode == 2)
	        {
		        if (resultCode == Result.Ok)
		        {
			        _takeAddressFromSearch = true;
			        _pickupRadio.Checked = false;
			        _destitationRadio.Checked = false;

			        Place place = Autocomplete.GetPlaceFromIntent(data);
			        _destinationText.Text = place.Name;
			        _destinationLatLng = place.LatLng;
			        _destinationAddress = place.Name;
			        _googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(place.LatLng, 15));
			        _centerMarker.SetColorFilter(Color.Red);
			        TripLocationsSet();
		        }
	        }
        }

		#endregion

		#region Click Event Handlesrs

		private void LayoutPickUp_Click(object sender, EventArgs e)
        {
	        List<Place.Field> fields = new List<Place.Field>
            {
	            Place.Field.Id, Place.Field.Name, Place.Field.LatLng, Place.Field.Address
            };

            Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
	            .SetCountry("UA")
	            .Build(this);

            StartActivityForResult(intent, 1);
        }

        private void LayoutDestination_Click(object sender, EventArgs e)
        {
	        List<Place.Field> fields = new List<Place.Field>
	        {
		        Place.Field.Id, Place.Field.Name, Place.Field.LatLng, Place.Field.Address
	        };

	        Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
		        .SetCountry("UA")
		        .Build(this);

	        StartActivityForResult(intent, 2);
        }

        private void PickupRadio_Click(object sender, EventArgs e)
        {
	        _addressRequest = 1;
	        _pickupRadio.Checked = true;
			_destitationRadio.Checked = false;
			_takeAddressFromSearch = false;
			_centerMarker.SetColorFilter(Color.DarkGreen);
        }

        private void DestinationRadio_Click(object sender, EventArgs e)
        {
	        _addressRequest = 2;
	        _destitationRadio.Checked = true;
			_pickupRadio.Checked = false;
			_takeAddressFromSearch = false;
			_centerMarker.SetColorFilter(Color.Red);
		}

        private void FavouritePlacesButoon_Click(object sender, EventArgs e)
        {

        }

        private async void LocationSetButton_Click(object sender, EventArgs e)
        {
	        _locationSetButton.Text = "Plsease wait...";
			_locationSetButton.Enabled = false;

			string json = await _mapHelper.GetDirectionJsonAsync(_pickupLocationLatlng, _destinationLatLng);

			if (!string.IsNullOrEmpty(json))
			{
				TextView txtFare = FindViewById<TextView>(Resource.Id.tripEstimateFareText);
				TextView txtTime = FindViewById<TextView>(Resource.Id.newTripTimeText);

				_mapHelper.DrawTripOnMap(json);

				double estimatedFare = _mapHelper.GetEstimatedFare();

				txtFare.Text = $"{estimatedFare} - {estimatedFare + 20} ₴";
				txtTime.Text = _mapHelper.GetDuration();

				_tripDetailsBottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;

				TripDrawnOnMap();
			}

			_locationSetButton.Text = "Done";
			_locationSetButton.Enabled = true;
        }

        private async void RequestDriverButton_Click(object sender, EventArgs e)
        {
	        requestDriverFragment = new RequestDriver(_mapHelper.GetEstimatedFare());
			requestDriverFragment.Cancelable = false;

			FragmentTransaction fragmentTransaction = SupportFragmentManager.BeginTransaction();
			requestDriverFragment.Show(fragmentTransaction, "Request");
			requestDriverFragment.CancelRequest += RequestDriverFragment_CancelRequest;

			_newTripDetails = new NewTripDetails
			{
				DestinationAddress = _destinationAddress,
				PickupAddress = _pickupAddress,
				DestinationLat = _destinationLatLng.Latitude,
				DestinationLng = _destinationLatLng.Longitude,
				DistanceString = _mapHelper.distanceString,
				DistanceValue = _mapHelper.distance,
				DurationString = _mapHelper.durationString,
				DurationValue = _mapHelper.duration,
				EstimateFare = _mapHelper.GetEstimatedFare(),
				Paymentmethod = "cash",
				PickupLat = _pickupLocationLatlng.Latitude,
				PickupLng = _pickupLocationLatlng.Longitude,
				Timestamp = DateTime.Now
			};

			_requestEventListener = new CreateRequestEventListener(_newTripDetails);
			await _requestEventListener.CreateRequestAsync();
        }

        private async void RequestDriverFragment_CancelRequest(object sender, EventArgs e)
        {
	        if (requestDriverFragment == null || _requestEventListener == null)
	        {
		        return;
	        }

	        await _requestEventListener.CancelRequestAsync();
	        requestDriverFragment.Dismiss();

	        _requestEventListener = null;
	        requestDriverFragment = null;
        }

		#endregion

		#region Map And Location

		public void OnMapReady(GoogleMap googleMap)
		{
			_googleMap = googleMap;
			_googleMap.CameraIdle += MainMap_CameraIdle;

			string mapKey = Resources.GetString(Resource.String.mapKey);
			_mapHelper = new MapFunctionHelper(mapKey, _googleMap);
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

        private void InitializePlaces()
        {
	        string mapKey = Resources.GetString(Resource.String.mapKey);

	        if (!PlacesApi.IsInitialized)
	        {
		        PlacesApi.Initialize(this, mapKey);
	        }
        }

        private async void MainMap_CameraIdle(object sender, EventArgs e)
        {
	        if (!_takeAddressFromSearch)
	        {
		        if (_addressRequest == 1)
		        {
			        _pickupLocationLatlng = _googleMap.CameraPosition.Target;
			        _pickupAddress = await _mapHelper.FindCordinateAddress(_pickupLocationLatlng);
			        _pickupLocationText.Text = _pickupAddress;
		        }
		        else if (_addressRequest == 2)
		        {
			        _destinationLatLng = _googleMap.CameraPosition.Target;
			        _destinationAddress = await _mapHelper.FindCordinateAddress(_destinationLatLng);
			        _destinationText.Text = _destinationAddress;

					TripLocationsSet();
		        }
	        }
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

        private bool CheckLocationPermissions()
        {
	        bool permissionsGranted = _permissionGroupLocation.All(x => ContextCompat.CheckSelfPermission(this, x) == Permission.Granted);

	        if (!permissionsGranted)
	        {
		        RequestPermissions(_permissionGroupLocation, RequestLocationId);
	        }

	        return permissionsGranted;
        }

		#endregion

		#region Trip Configurations

		private void TripLocationsSet()
		{
			_favouritePlacesButton.Visibility = ViewStates.Invisible;
			_locationSetButton.Visibility = ViewStates.Visible;
		}

		private void TripDrawnOnMap()
		{
			_layoutDestination.Clickable = false;
			_layoutPickUp.Clickable = false;

			_pickupRadio.Enabled = false;
			_destitationRadio.Enabled = false;
			_takeAddressFromSearch = false;
			_centerMarker.Visibility = ViewStates.Invisible;
		}

		#endregion
	}
}