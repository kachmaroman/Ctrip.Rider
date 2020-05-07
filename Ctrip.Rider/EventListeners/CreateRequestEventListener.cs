using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.Gms.Maps.Model;
using Ctrip.Rider.DataModels;
using Ctrip.Rider.Helpers;
using Firebase.Database;
using Java.Util;

namespace Ctrip.Rider.EventListeners
{
    public class CreateRequestEventListener : Java.Lang.Object, IValueEventListener
    {
	    readonly NewTripDetails _newTrip;
	    readonly FirebaseDatabase _database;
        DatabaseReference _newTripRef;
        DatabaseReference _notifyDriverRef;

        //NotifyDriver
        List<AvailableDriver> _mAvailableDrivers;
        AvailableDriver _selectedDriver;

        readonly System.Timers.Timer _requestTimer = new System.Timers.Timer();
        int _timerCounter = 0;
        bool _isDriverAccepted;


        public class DriverAcceptedEventArgs : EventArgs
        {
            public AcceptedDriver acceptedDriver { get; set; }
        }

        public class TripUpdatesEventArgs : EventArgs
        {
            public LatLng DriverLocation { get; set; }
            public string Status { get; set; }
            public double Fares { get; set; }
        }

        public event EventHandler<DriverAcceptedEventArgs> DriverAccepted;
        public event EventHandler NoDriverAcceptedRequest;
        public event EventHandler<TripUpdatesEventArgs> TripUpdates;


        public void OnCancelled(DatabaseError error)
        {

        }

        public void OnDataChange(DataSnapshot snapshot)
        {
            if (snapshot.Value != null)
            {
                if (snapshot.Child("driver_id").Value.ToString() != "waiting")
                {
                    string status = "";
                    double fares = 0;

                    if (!_isDriverAccepted)
                    {
                        AcceptedDriver acceptedDriver = new AcceptedDriver();
                        acceptedDriver.Id = snapshot.Child("driver_id").Value.ToString();
                        acceptedDriver.Fullname = snapshot.Child("driver_name").Value.ToString();
                        acceptedDriver.Phone = snapshot.Child("driver_phone").Value.ToString();
                        _isDriverAccepted = true;
                        DriverAccepted?.Invoke(this, new DriverAcceptedEventArgs { acceptedDriver = acceptedDriver });
                    }

                    //Gets Status
                    if (snapshot.Child("status").Value != null)
                    {
                        status = snapshot.Child("status").Value.ToString();
                    }

                    //Get Fares
                    if (snapshot.Child("fares").Value != null)
                    {
                        fares = double.Parse(snapshot.Child("fares").Value.ToString());
                    }

                    if (_isDriverAccepted)
                    {
                        //Get Driver Location Updates
                        double driverLatitude = double.Parse(snapshot.Child("driver_location").Child("latitude").Value.ToString());
                        double driverLongitude = double.Parse(snapshot.Child("driver_location").Child("longitude").Value.ToString());
                        LatLng driverLocationLatLng = new LatLng(driverLatitude, driverLongitude);
                        TripUpdates?.Invoke(this, new TripUpdatesEventArgs { DriverLocation = driverLocationLatLng, Status = status, Fares = fares });
                    }
                }
            }
        }

        public CreateRequestEventListener(NewTripDetails mNewTrip)
        {
            _newTrip = mNewTrip;
            _database = AppDataHelper.GetDatabase();

            _requestTimer.Interval = 1000;
            _requestTimer.Elapsed += RequestTimer_Elapsed;
        }

        public async Task CreateRequestAsync()
        {
	        _newTripRef = _database.GetReference("Ride_requests").Push();

            HashMap pickup = new HashMap();

	        pickup.Put("latitude", _newTrip.PickupLat);
	        pickup.Put("longitude", _newTrip.PickupLng);

	        HashMap destination = new HashMap();
	        destination.Put("latitude", _newTrip.DestinationLat);
	        destination.Put("longitude", _newTrip.DestinationLng);

	        HashMap trip = new HashMap();
	        _newTrip.RideId = _newTripRef.Key;

	        trip.Put("rider_id", AppDataHelper.GetCurrentUser().Uid);

	        trip.Put("pickup_address", _newTrip.PickupAddress);
	        trip.Put("pickup", pickup);

	        trip.Put("destination", destination);
	        trip.Put("destination_address", _newTrip.DestinationAddress);

	        trip.Put("distance", _newTrip.DistanceString);
	        trip.Put("duration", _newTrip.DurationString);
	        trip.Put("ride_fare", _newTrip.EstimateFare);

	        trip.Put("driver_id", "waiting");

	        trip.Put("payment_method", _newTrip.Paymentmethod);

	        trip.Put("rider_name", AppDataHelper.GetFullName());
	        trip.Put("rider_phone", AppDataHelper.GetPhone());

	        trip.Put("created_at", _newTrip.Timestamp.ToString(CultureInfo.InvariantCulture));

	        _newTripRef.AddValueEventListener(this);

	        await _newTripRef.SetValueAsync(trip);
        }

        public async Task CancelRequestAsync()
        {
            if (_selectedDriver != null)
            {
                DatabaseReference cancelDriverRef = _database.GetReference("driversAvailable/" + _selectedDriver.Id + "/ride_id");
                cancelDriverRef.SetValue("cancelled");
            }

            _newTripRef.RemoveEventListener(this);
            await _newTripRef.RemoveValueAsync();
        }

        public void CancelRequestOnTimeout()
        {
            _newTripRef.RemoveEventListener(this);
            _newTripRef.RemoveValue();
        }

        public void NotifyDriver(List<AvailableDriver> availableDrivers)
        {
            _mAvailableDrivers = availableDrivers;
            if (_mAvailableDrivers.Count >= 1 && _mAvailableDrivers != null)
            {
                _selectedDriver = _mAvailableDrivers[0];
                _notifyDriverRef = _database.GetReference("driversAvailable/" + _selectedDriver.Id + "/ride_id");
                _notifyDriverRef.SetValue(_newTrip.RideId);

                if (_mAvailableDrivers.Count > 1)
                {
                    _mAvailableDrivers.RemoveAt(0);
                }
                else if (_mAvailableDrivers.Count == 1)
                {
	                _mAvailableDrivers = null;
                }

                _requestTimer.Enabled = true;
            }
            else
            {
	            _requestTimer.Enabled = false;
                NoDriverAcceptedRequest?.Invoke(this, new EventArgs());
            }
        }

        private void RequestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timerCounter++;

            if (_timerCounter == 100)
            {
                if (!_isDriverAccepted)
                {
                    _timerCounter = 0;
                    DatabaseReference cancelDriverRef = _database.GetReference("driversAvailable/" + _selectedDriver.Id + "/ride_id");
                    cancelDriverRef.SetValue("timeout");

                    if (_mAvailableDrivers != null)
                    {
                        NotifyDriver(_mAvailableDrivers);
                    }
                    else
                    {
                        _requestTimer.Enabled = false;
                        NoDriverAcceptedRequest?.Invoke(this, new EventArgs());
                    }
                }
            }
        }

        public void EndTrip()
        {
            _newTripRef.RemoveEventListener(this);
            _newTripRef = null;
        }
    }
}