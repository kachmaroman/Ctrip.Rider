using System;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Ctrip.Rider.Fragments
{
    public class RequestDriver : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler CancelRequest;

        private readonly double _mfare;
        private Button _cancelRequestButton;
        private TextView _faresText;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
	        View view = inflater.Inflate(Resource.Layout.request_driver, container, false);
            _cancelRequestButton = view.FindViewById<Button>(Resource.Id.cancelRequestButton);
            _cancelRequestButton.Click += CancelRequestButton_Click;
            _faresText = view.FindViewById<TextView>(Resource.Id.faresText);
            _faresText.Text = $"{_mfare}₴";

            return view;
        }

        public RequestDriver(double mfare)
        {
	        _mfare = mfare;
        }

        private void CancelRequestButton_Click(object sender, EventArgs e)
        {
            CancelRequest?.Invoke(this, new EventArgs());
        }
    }
}