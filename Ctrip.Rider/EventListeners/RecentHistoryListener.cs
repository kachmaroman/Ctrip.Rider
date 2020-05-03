using System;
using System.Collections.Generic;
using System.Linq;
using Ctrip.Rider.DataModels;
using Ctrip.Rider.Helpers;
using Firebase.Database;

namespace Ctrip.Rider.EventListeners
{
    public class RecentHistoryListener : Java.Lang.Object, IValueEventListener
    {
	    private readonly List<NewTripDetails> _recentTripList = new List<NewTripDetails>();

        public event EventHandler<RecentTripEventArgs> HistoryRetrieved;

        public class RecentTripEventArgs : EventArgs
        {
            public List<NewTripDetails> RecentTripList { get; set; }
        }

        public void OnCancelled(DatabaseError error)
        {
            
        }

        public void OnDataChange(DataSnapshot snapshot)
        {
	        if (snapshot.Value == null)
	        {
		        return;
	        }

	        var child = snapshot.Children.ToEnumerable<DataSnapshot>();

            _recentTripList.Clear();

            foreach (DataSnapshot searchData in child)
            {
	            NewTripDetails tripDetails = new NewTripDetails
	            {
		            RideId = searchData.Key,
                    EstimateFare = (double)searchData.Child("ride_fare").Value,
                    Timestamp = DateTime.Parse((string)searchData.Child("created_at").Value),
                    PickupLat = (double)searchData.Child("pickup").Child("latitude").Value,
		            PickupLng = (double)searchData.Child("pickup").Child("longitude").Value,
		            DestinationLat = (double)searchData.Child("destination").Child("latitude").Value,
		            DestinationLng = (double)searchData.Child("destination").Child("longitude").Value,
		            PickupAddress = searchData.Child("pickup_address").Value.ToString(),
		            DestinationAddress = searchData.Child("destination_address").Value.ToString()
	            };

	            _recentTripList.Add(tripDetails);
            }

            HistoryRetrieved?.Invoke(this, new RecentTripEventArgs { RecentTripList = _recentTripList });
        }

        public void Create()
        {
            AppDataHelper.GetDatabase().GetReference("Ride_requests").AddValueEventListener(this);
        }
    }
}