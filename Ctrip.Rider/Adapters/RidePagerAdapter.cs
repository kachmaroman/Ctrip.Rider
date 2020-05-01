using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Ctrip.Rider.DataModels;

namespace Ctrip.Rider.Adapters
{
    public class RidePagerAdapter : PagerAdapter
    {
	    private readonly Activity _activity;
        readonly List<RideTypeDataModel> _rideTypeList;
        LayoutInflater _inflater;

        public RidePagerAdapter(Activity activity, List<RideTypeDataModel> rideTypeList)
        {
            _activity = activity;
            _rideTypeList = rideTypeList;
        }
        public override int Count => _rideTypeList.Count;

        public override bool IsViewFromObject(View view, Java.Lang.Object @object) => view.Equals(@object);

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            _inflater = LayoutInflater.From(_activity);

            View view = _inflater.Inflate(Resource.Layout.ride_type_layout, container, false);
            ImageView imageView = view.FindViewById<ImageView>(Resource.Id.ride_img);
            TextView titleText = view.FindViewById<TextView>(Resource.Id.txt_ride_type);
            TextView descText = view.FindViewById<TextView>(Resource.Id.txt_ride_price);
            TextView durationText = view.FindViewById<TextView>(Resource.Id.txt_ride_duratioon);

            imageView.SetImageResource(_rideTypeList[position].Image);

            titleText.Text = _rideTypeList[position].RideType;
            descText.Text = _rideTypeList[position].RidePrice;
            durationText.Text = _rideTypeList[position].RiderDuration;

            container.AddView(view, 0);

            return view;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
        {
            container.RemoveView((View)@object);
        }
    }
}