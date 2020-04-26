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
			_newTripRef = _database.GetReference("riderRequest").Push();

			HashMap pickup = new HashMap();
			pickup.Put("latitude", _newTrip.PickupLat);
			pickup.Put("longitude", _newTrip.PickupLng);

			HashMap destination = new HashMap();
			destination.Put("latitude", _newTrip.DestinationLat);
			destination.Put("longitude", _newTrip.DestinationLng);

			HashMap trip = new HashMap();
			_newTrip.RideId = _newTripRef.Key;

			trip.Put("pickup", pickup);
			trip.Put("destination", destination);
			trip.Put("pickup_address", _newTrip.PickupAddress);
			trip.Put("destination_address", _newTrip.DestinationAddress);
			trip.Put("rider_id", AppDataHelper.GetCurrentUser().Uid);
			trip.Put("created", _newTrip.Timestamp.ToString(CultureInfo.InvariantCulture));
			trip.Put("driver_id", "waiting");
			trip.Put("rider_name", AppDataHelper.GetFullName());
			trip.Put("rider_phone", AppDataHelper.GetPhone());

			_newTripRef.AddValueEventListener(this);
			await _newTripRef.SetValueAsync(trip);
		}

		public async Task CancelRequestAsync()
		{
			_newTripRef.RemoveEventListener(this);
			await _newTripRef.RemoveValueAsync();
		}
	}
}