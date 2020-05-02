using System.Globalization;
using System.Threading.Tasks;
using Ctrip.Rider.DataModels;
using Ctrip.Rider.Helpers;
using Firebase.Database;
using Java.Util;

namespace Ctrip.Rider.EventListeners
{
	public class CreateRequestEventListener : Java.Lang.Object, IValueEventListener
	{
		private NewTripDetails _newTrip;
		private FirebaseDatabase _database;
		private DatabaseReference _newTripRef;

		public CreateRequestEventListener(NewTripDetails newTrip)
		{
			_newTrip = newTrip;
			_database = AppDataHelper.GetDatabase();
		}

		public void OnCancelled(DatabaseError error)
		{

		}

		public void OnDataChange(DataSnapshot snapshot)
		{

		}

		public async Task CreateRequestAsync()
		{
			_newTripRef = _database.GetReference("users").Child(AppDataHelper.GetCurrentUser().Uid).Child("Ride_requests").Push();

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

		public void CancelRequestAsync()
		{
			_newTripRef.RemoveEventListener(this);
			_newTripRef.RemoveValue();
		}
	}
}