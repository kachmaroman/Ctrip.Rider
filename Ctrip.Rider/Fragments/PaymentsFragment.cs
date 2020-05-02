using System;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace Ctrip.Rider.Fragments
{
    public class PaymentsFragment : Android.Support.V4.App.Fragment
    {
        private BottomSheetBehavior _mBehavior;
        private RelativeLayout _mBottomsheetRoot;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.payments_layout, container, false);

            _mBottomsheetRoot = view.FindViewById<RelativeLayout>(Resource.Id.payment_sheet_root);
            _mBehavior = BottomSheetBehavior.From(_mBottomsheetRoot);
            _mBehavior.State = BottomSheetBehavior.StateHidden;

            TextView cancelTxt = view.FindViewById<TextView>(Resource.Id.cancel_txt);
            cancelTxt.Click += CancelTxt_Click;

            Button addPaymentBtn = view.FindViewById<Button>(Resource.Id.add_new_payment_btn);
            addPaymentBtn.Click += AddPaymentBtn_Click;

            RelativeLayout momoBtn = view.FindViewById<RelativeLayout>(Resource.Id.momo_relative);
            momoBtn.Click += MomoBtn_Click;

            RelativeLayout ccBtn = view.FindViewById<RelativeLayout>(Resource.Id.cc_relative);
            ccBtn.Click += CcBtn_Click;

            return view;
        }

        private void CcBtn_Click(object sender, EventArgs e)
        {
            Toast.MakeText(Context, "Credit card added successfully", ToastLength.Short).Show();

        }

        private void MomoBtn_Click(object sender, EventArgs e)
        {
            Toast.MakeText(Context, "Momo added successfully", ToastLength.Short).Show();
        }

        private void CancelTxt_Click(object sender, EventArgs e)
        {
            _mBehavior.Hideable = true;
            _mBehavior.State = BottomSheetBehavior.StateHidden;
        }

        private void AddPaymentBtn_Click(object sender, EventArgs e)
        {
            _mBehavior.State = BottomSheetBehavior.StateExpanded;
            _mBehavior.Hideable = false;
        }
    }
}