using System;
using Xamarin.Facebook;

namespace Ctrip.Rider.EventListeners
{
	public class FacebookProfileEventListener : ProfileTracker
	{
		public event EventHandler<OnProfileChangedEventArgs> OnProfileChanged;

		protected override void OnCurrentProfileChanged(Profile oldProfile, Profile newProfile)
		{
			OnProfileChanged?.Invoke(this, new OnProfileChangedEventArgs(newProfile));
		}
	}

	public class OnProfileChangedEventArgs : EventArgs
	{
		public Profile mProfile;

		public OnProfileChangedEventArgs(Profile profile) { mProfile = profile; }
	}
}