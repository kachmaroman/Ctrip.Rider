using Android.App;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Views;
using Android.Widget;

namespace Ctrip.Rider.Helpers
{
    public class InfoWindowHelper : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
    {
        private readonly Activity _context;
        private readonly View _view;
        private Marker _mMarker;

        public InfoWindowHelper(Activity context)
        {
	        _context = context;
	        _view = context.LayoutInflater.Inflate(Resource.Layout.info_window_layout, null);
        }

        public View GetInfoContents(Marker marker)
        {
            if (marker == null)
                return null;

            _mMarker = marker;

            TextView textTime = _view.FindViewById<TextView>(Resource.Id.m_tv_1);
            TextView textLocation = _view.FindViewById<TextView>(Resource.Id.info_txt);
            LinearLayout linear = _view.FindViewById<LinearLayout>(Resource.Id.linearLayout1);

            textTime.Text = marker.Snippet;
            textLocation.Text = marker.Title;

            return _view;
        }

        public View GetInfoWindow(Marker marker)
        {
            return null;
        }
    }
}