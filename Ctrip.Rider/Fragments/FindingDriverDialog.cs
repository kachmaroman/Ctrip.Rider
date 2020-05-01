using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;

namespace Ctrip.Rider.Fragments
{
    public class FindingDriverDialog : Android.Support.V4.App.DialogFragment
    {
        private BottomSheetBehavior _behaviorFinder;
        private RelativeLayout _rlBottomSheet;

        private TextView _fromTv;
        private TextView _toTv;
        private TextView _timeTv;
        private TextView _priceTv;

        private static string _from, _to, _time;
        private static double _fares;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetStyle(StyleNormal, Resource.Style.FullScreenDialogTheme);
        }

        public static FindingDriverDialog Display(Android.Support.V4.App.FragmentManager fragmentManager, bool cancelable, string from, string to, string time, double fares)
        {
	        FindingDriverDialog findingDriver = new FindingDriverDialog
            {
                Cancelable = cancelable
            };

            _from = from;
            _to = to;
            _time = time;
            _fares = fares;

            findingDriver.Show(fragmentManager, "TAG");

            return findingDriver;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.finding_driver_nearby, container, false);

            LinearLayout mlinear = view.FindViewById<LinearLayout>(Resource.Id.mLinear_finder);
            mlinear.Click += Mlinear__Click;

            _fromTv = (TextView)view.FindViewById(Resource.Id.mypos_tv);
            _toTv = (TextView)view.FindViewById(Resource.Id.dest_tv);
            _timeTv = (TextView)view.FindViewById(Resource.Id.time_tv);
            _priceTv = (TextView)view.FindViewById(Resource.Id.price_tv);

            _fromTv.Text = _from;
            _toTv.Text = _to;
            _timeTv.Text = _time;
            _priceTv.Text = $"₴ {_fares}";

            Button btnCancel = view.FindViewById<Button>(Resource.Id.canc_btn);

            btnCancel.Click += (sender, e) =>
            {
                MainActivity.Instance.ReverseTrip();
                MainActivity.Instance.RequestEventListener.CancelRequestAsync();
                
                Dismiss();
            };

            _rlBottomSheet = (RelativeLayout)view.FindViewById(Resource.Id.finder_rl);
            _behaviorFinder = BottomSheetBehavior.From(_rlBottomSheet);

            return view;
        }

        private void Mlinear__Click(object sender, EventArgs e)
        {
	        _behaviorFinder.State = _behaviorFinder.State switch
	        {
		        BottomSheetBehavior.StateExpanded => BottomSheetBehavior.StateCollapsed,
		        BottomSheetBehavior.StateCollapsed => BottomSheetBehavior.StateExpanded,
		        _ => BottomSheetBehavior.StateCollapsed
	        };
        }
    }
}