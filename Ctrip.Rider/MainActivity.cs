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
using Android.OS;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using Ctrip.Rider.Adapters;
using Ctrip.Rider.DataModels;
using Ctrip.Rider.EventListeners;
using Ctrip.Rider.Fragments;
using Ctrip.Rider.Helpers;
using FFImageLoading;
using Google.Places;
using Java.Util;
using Refractored.Controls;
using ActionBar = Android.Support.V7.App.ActionBar;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using static Android.Support.Design.Widget.NavigationView;
using Location = Android.Locations.Location;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using static Android.Support.V4.View.ViewPager;
using Result = Android.App.Result;

namespace Ctrip.Rider
{
	[Activity(Label = "@string/app_name", Theme = "@style/CtripTheme", MainLauncher = false)]
	public class MainActivity : AppCompatActivity, IOnMapReadyCallback, IOnPageChangeListener, IOnNavigationItemSelectedListener
	{
		private readonly UserProfileEventListener _profileEventListener = new UserProfileEventListener();

		public CreateRequestEventListener RequestEventListener { get; set; }

		//Views
		private Toolbar _mainToolbar;
		private DrawerLayout _drawerLayout;
		private NavigationView _navView;

		//TextViews
		private TextView _accountTitleText;
		private TextView _fromLocationText;
		private TextView _toLocationText;
		private TextView _pickupText;
		private TextView _destinationText;
		private TextView _greetings_tv;
		private TextView _drawerTextUsername;

		//Progresses
		private ProgressBar _pickupProgress;
		private ProgressBar _destinationProgress;

		//Layouts
		private RelativeLayout _bottomSheetRootView;
		private RelativeLayout _tripDetailsView;
		private RelativeLayout _layoutPickUp;
		private RelativeLayout _layoutDestination;

		//Bottom-sheets
		private BottomSheetBehavior _bottomSheetRootBehavior;
		private BottomSheetBehavior _tripDetailsBehavior;

		//Buttons
		private FloatingActionButton _myLocation;
		private Button _requestRideBnt;

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
		private static readonly float Zoom = 16.0f;

		//Helpers
		MapFunctionHelper _mapHelper;

		//TripDetails
		private LatLng _pickupLocationLatlng;
		private LatLng _destinationLatLng;
		private string _pickupAddress;
		private string _destinationAddress;
		private ViewPager _viewPager;
		private PagerAdapter _pagerAdapter;
		private List<RideTypeDataModel> _rideTypeList;

		//Flags
		//private bool _takeAddressFromSearch = false;
		private bool _isTripDrawn = false;

		//DataModels
		private NewTripDetails _newTripDetails;

		//Constants
		private const int RequestCodePickup = 1;
		private const int RequestCodeDestination = 2;

		internal static MainActivity Instance { get; set; }

		private void ConnectControls()
		{
			_drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawerLayout);
			_mainToolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar);

			SetSupportActionBar(_mainToolbar);

			SupportActionBar.Title = string.Empty;

			ActionBar actionBar = SupportActionBar;
			actionBar.SetHomeAsUpIndicator(Resource.Mipmap.ic_menu_action);
			actionBar.SetDisplayHomeAsUpEnabled(true);

			_fromLocationText = FindViewById<TextView>(Resource.Id.from_tv);
			_toLocationText = FindViewById<TextView>(Resource.Id.to_tv);
			_pickupText = FindViewById<TextView>(Resource.Id.pickupText);
			_destinationText = FindViewById<TextView>(Resource.Id.destinationText);
			_greetings_tv = FindViewById<TextView>(Resource.Id.greetings_tv);
			_layoutPickUp = FindViewById<RelativeLayout>(Resource.Id.layoutPickup);
			_layoutDestination = FindViewById<RelativeLayout>(Resource.Id.layoutDestination);
			_layoutPickUp.Click += (sender, e) => StartAutoComplete(RequestCodePickup);
			_layoutDestination.Click += (sender, e) => StartAutoComplete(RequestCodeDestination);

			_pickupProgress = FindViewById<ProgressBar>(Resource.Id.pickupProgress);
			_destinationProgress = FindViewById<ProgressBar>(Resource.Id.destiopnationProgress);

			_myLocation = FindViewById<FloatingActionButton>(Resource.Id.fab_myloc);
			_requestRideBnt = FindViewById<Button>(Resource.Id.ride_select_btn);

			_myLocation.Click += MyLocation_Click;
			_requestRideBnt.Click += RequestRideBntClick;

			_viewPager = FindViewById<ViewPager>(Resource.Id.viewPager);

			_bottomSheetRootView = FindViewById<RelativeLayout>(Resource.Id.main_sheet_root);
			_tripDetailsView = FindViewById<RelativeLayout>(Resource.Id.trip_root);
			_bottomSheetRootBehavior = BottomSheetBehavior.From(_bottomSheetRootView);
			_tripDetailsBehavior = BottomSheetBehavior.From(_tripDetailsView);

			_bottomSheetRootBehavior.PeekHeight = BottomSheetBehavior.PeekHeightAuto;
			_bottomSheetRootBehavior.State = BottomSheetBehavior.StateHidden;

			if (!_isTripDrawn)
			{
				_tripDetailsBehavior.State = BottomSheetBehavior.StateHidden;
			}

			_greetings_tv.Text = GetGreetings();

			_navView = FindViewById<NavigationView>(Resource.Id.navView);
			_navView.ItemIconTintList = null;

			View headerView = _navView.GetHeaderView(0);
			headerView.Click += HeaderView_Click;

			_drawerTextUsername = headerView.FindViewById<TextView>(Resource.Id.accountTitle);
			_drawerTextUsername.Text = AppDataHelper.GetFullName();

			CircleImageView accountImage = headerView.FindViewById<CircleImageView>(Resource.Id.accountImage);

			RunOnUiThread(() =>
			{
				SetProfilePic(AppDataHelper.GetFbProfilePic(), accountImage);
			});

			SetUpDrawerContent(_navView);
		}

		#region Overrides

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);

			Instance = this;

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

			_rideTypeList = new List<RideTypeDataModel>();
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

		protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == RequestCodePickup)
			{
				if (resultCode == Result.Ok)
				{
					_pickupProgress.Visibility = ViewStates.Visible;

					Place place = Autocomplete.GetPlaceFromIntent(data);
					_pickupText.Text = place.Name;
					_fromLocationText.Text = place.Name;
					_pickupLocationLatlng = place.LatLng;
					_pickupAddress = place.Name;
					_layoutPickUp.Enabled = false;

					_pickupProgress.Visibility = ViewStates.Gone;
				}
				else if (resultCode == Result.Canceled)
				{
					_pickupProgress.Visibility = ViewStates.Gone;
				}
			}

			if (requestCode == RequestCodeDestination)
			{
				if (resultCode == Result.Ok)
				{
					_destinationProgress.Visibility = ViewStates.Visible;

					Place place = Autocomplete.GetPlaceFromIntent(data);
					_destinationText.Text = place.Name;
					_destinationLatLng = place.LatLng;
					_destinationAddress = place.Name;
					_layoutDestination.Enabled = false;

					await TripLocationsSet();
				}
				else if (resultCode == Result.Canceled)
				{
					_destinationProgress.Visibility = ViewStates.Gone;
				}
			}
		}

		public override void OnBackPressed()
		{
			if (_drawerLayout.IsDrawerOpen((int)GravityFlags.Start))
			{
				_drawerLayout.CloseDrawer((int)GravityFlags.Start);
			}
			else
			{
				if (_isTripDrawn)
				{
					ResetTrip();
				}
				else
				{
					base.OnBackPressed();
				}
			}
		}
		public void OnPageScrollStateChanged(int state)
		{ }

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{ }

		public void OnPageSelected(int position)
		{ }

		public bool OnNavigationItemSelected(IMenuItem menuItem)
		{
			SelectDrawerItem(menuItem.ItemId);
			return true;
		}

		private void SetUpDrawerContent(NavigationView navView)
		{
			navView.SetNavigationItemSelectedListener(this);
		}

		#endregion

		#region Click Event Handlesrs

		private void HeaderView_Click(object sender, EventArgs e)
		{
			if (!_drawerLayout.IsDrawerOpen((int) GravityFlags.Start))
			{
				return;
			}

			Android.Support.V4.App.Fragment profileFragment = new ProfileFragment();

			SupportFragmentManager
				.BeginTransaction()
				.SetCustomAnimations(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out)
				.Replace(Resource.Id.content_frame, profileFragment, profileFragment.Class.SimpleName)
				.AddToBackStack(null)
				.Commit();

			_drawerLayout.CloseDrawer((int)GravityFlags.Start);
		}

		private async void MyLocation_Click(object sender, EventArgs e)
		{
			if (_pickupLocationLatlng != null && !_isTripDrawn)
			{
				_googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(_pickupLocationLatlng, Zoom));
			}
			else if (_pickupLocationLatlng == null && !_isTripDrawn)
			{
				await GetCurrentLocationAsync();
			}
		}

		private async void RequestRideBntClick(object sender, EventArgs e)
		{
			_tripDetailsBehavior.Hideable = true;
			_bottomSheetRootBehavior.Hideable = true;
			_tripDetailsBehavior.State = BottomSheetBehavior.StateHidden;
			_bottomSheetRootBehavior.State = BottomSheetBehavior.StateHidden;

			FindingDriverDialog.Display(SupportFragmentManager, false, _pickupAddress, _destinationAddress, _mapHelper.durationString, _mapHelper.GetEstimatedFare());

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

			RequestEventListener = new CreateRequestEventListener(_newTripDetails);
			await RequestEventListener.CreateRequestAsync();
		}

		#endregion

		#region Map And Location

		public async void OnMapReady(GoogleMap googleMap)
		{
			_googleMap = googleMap;
			_googleMap.MyLocationEnabled = true;
			_googleMap.UiSettings.MyLocationButtonEnabled = false;
			_googleMap.UiSettings.CompassEnabled = false;
			_googleMap.UiSettings.RotateGesturesEnabled = false;
			_googleMap.UiSettings.MapToolbarEnabled = false;

			_pickupProgress.Visibility = ViewStates.Visible;

			await GetCurrentLocationAsync();

			_mapHelper = new MapFunctionHelper(Resources.GetString(Resource.String.mapKey), _googleMap);

			_pickupLocationLatlng = _googleMap.CameraPosition.Target;
			_pickupAddress = await _mapHelper.FindCordinateAddress(_pickupLocationLatlng);

			_pickupText.Text = _pickupAddress;
			_fromLocationText.Text = _pickupAddress;
			_pickupProgress.Visibility = ViewStates.Gone;
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
			_googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(currentPosition, Zoom));
			SetTripUi();
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

		private void StartAutoComplete(int requestCode)
		{
			List<Place.Field> fields = new List<Place.Field>
			{
				Place.Field.Id, Place.Field.Name, Place.Field.LatLng, Place.Field.Address
			};

			Intent intent = new Autocomplete.IntentBuilder(AutocompleteActivityMode.Overlay, fields)
				.SetCountry("UA")
				.Build(this);

			StartActivityForResult(intent, requestCode);
		}

		private async Task TripLocationsSet()
		{
			string json = await _mapHelper.GetDirectionJsonAsync(_pickupLocationLatlng, _destinationLatLng);

			if (!string.IsNullOrEmpty(json))
			{
				_isTripDrawn = true;

				RunOnUiThread(() =>
				{
					_mapHelper.DrawTripOnMap(json);

					double estimatedFare = _mapHelper.GetEstimatedFare();
					string duration = _mapHelper.GetDuration();

					_rideTypeList.Clear();
					_rideTypeList.Add(new RideTypeDataModel { Image = Resource.Drawable.taxi_lite, RideType = "Lite", RidePrice = $"₴ {estimatedFare}", RiderDuration = duration });
					_rideTypeList.Add(new RideTypeDataModel { Image = Resource.Drawable.taxi_standard, RideType = "Standard", RidePrice = $"₴ {estimatedFare + 20}", RiderDuration = duration });
					_rideTypeList.Add(new RideTypeDataModel { Image = Resource.Drawable.taxi_comfort, RideType = "Comfort", RidePrice = $"₴ {estimatedFare + 40}", RiderDuration = duration });
					_rideTypeList.Add(new RideTypeDataModel { Image = Resource.Drawable.taxi_minibus, RideType = "Minibus", RidePrice = $"₴ {estimatedFare + 80}", RiderDuration = duration });
					_rideTypeList.Add(new RideTypeDataModel { Image = Resource.Drawable.taxi_driver, RideType = "Driver", RidePrice = $"₴ {estimatedFare * 3}", RiderDuration = duration });

					_pagerAdapter = new RidePagerAdapter(this, _rideTypeList);
					_viewPager.Adapter = _pagerAdapter;
					_viewPager.AddOnPageChangeListener(this);

					_bottomSheetRootBehavior.Hideable = true;
					_bottomSheetRootBehavior.State = BottomSheetBehavior.StateHidden;

					_tripDetailsBehavior.State = BottomSheetBehavior.StateExpanded;
					_tripDetailsBehavior.Hideable = false;

					_destinationProgress.Visibility = ViewStates.Gone;

					_googleMap.SetPadding(0, 0, 0, _tripDetailsView.Height + 10);
				});
			}
		}

		private void SetTripUi()
		{
			_bottomSheetRootBehavior.State = BottomSheetBehavior.StateExpanded;
			_bottomSheetRootBehavior.Hideable = false;
			_myLocation.Visibility = ViewStates.Visible;
		}

		public void ReverseTrip()
		{
			_bottomSheetRootBehavior.Hideable = false;
			_bottomSheetRootBehavior.State = BottomSheetBehavior.StateExpanded;
		}

		private void ResetTrip()
		{
			if (!_isTripDrawn)
			{
				return;
			}

			_layoutPickUp.Enabled = true;
			_layoutDestination.Enabled = true;
			_pickupProgress.Visibility = ViewStates.Gone;
			_destinationProgress.Visibility = ViewStates.Gone;
			_greetings_tv.Text = GetGreetings();
			_destinationText.Text = "Where to go?";

			_isTripDrawn = false;
			_googleMap.Clear();

			_tripDetailsBehavior.Hideable = true;
			_bottomSheetRootBehavior.Hideable = false;
			_tripDetailsBehavior.State = BottomSheetBehavior.StateHidden;
			_bottomSheetRootBehavior.State = BottomSheetBehavior.StateExpanded;

			RunOnUiThread(() =>
			{
				_googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(_pickupLocationLatlng, 17.0f));
				_googleMap.SetPadding(0, 0, 0, _bottomSheetRootView.Height);
			});
		}

		#endregion

		private string GetGreetings()
		{
			string name = AppDataHelper.GetFirstname();
			string greeting = null;

			Date date = new Date();
			Calendar calendar = Calendar.Instance;
			calendar.Time = date;

			int hour = calendar.Get(CalendarField.HourOfDay);

			if (hour >= 12 && hour <= 18)
			{
				greeting = "Good Afternoon";
			}
			else if (hour > 18 && hour < 21)
			{
				greeting = "Good Evening";
			}
			else if (hour >= 21 && hour < 24)
			{
				greeting = "Good Night";
			}
			else
			{
				greeting = $"Good Morning, {name}";
			}

			return $"{greeting}, {name}";
		}

		private void SelectDrawerItem(int itemId)
		{
			Android.Support.V4.App.Fragment fragment = null;

			switch (itemId)
			{
				case Resource.Id.action_free_rides:
					break;
				case Resource.Id.action_payments:
					fragment = new PaymentsFragment();
					break;
				case Resource.Id.action_history:
					fragment = new PlacesHistory();
					break;
				case Resource.Id.action_promos:
					break;
				case Resource.Id.action_support:
					break;
				case Resource.Id.action_about:
					break;
			}

			if (fragment != null)
			{
				SupportFragmentManager
					.BeginTransaction()
					.SetCustomAnimations(Resource.Animation.slide_up_anim, Resource.Animation.slide_up_out)
					.Replace(Resource.Id.content_frame, fragment)
					.AddToBackStack(null)
					.Commit();
			}

			_drawerLayout.CloseDrawer(GravityCompat.Start);
		}

		private async void SetProfilePic(string providerId, CircleImageView accountImage)
		{
			try
			{
				await ImageService.Instance
					.LoadUrl($"https://graph.facebook.com/{providerId}/picture?type=normal")
					.LoadingPlaceholder("boy_new", FFImageLoading.Work.ImageSource.CompiledResource)
					.Retry(3, 200)
					.IntoAsync(accountImage);
			}
			catch
			{
				// ignored
			}
		}
	}
}