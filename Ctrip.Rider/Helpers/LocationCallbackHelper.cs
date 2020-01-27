using System;
using Android.Gms.Location;
using Android.Locations;
using Android.Util;

namespace Ctrip.Rider.Helpers
{
    public class LocationCallbackHelper : LocationCallback
    {
	    public event EventHandler<OnLocationCapturedEventArgs> CurrentLocation;

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("Ctrip", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }

        public override void OnLocationResult(LocationResult result)
        {
	        if (result.Locations.Count != 0)
	        {
		        CurrentLocation?.Invoke(this, new OnLocationCapturedEventArgs { Location = result.Locations[0] });
            }
        }
    }

    public class OnLocationCapturedEventArgs : EventArgs
    {
	    public Location Location { get; set; }
    }
}
