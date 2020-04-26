using Android.App;

namespace Ctrip.Rider.Helpers
{
    public class AlertDialogHelper : Java.Lang.Object
    {
        Activity _activity;
        //dialogs
        Android.Support.V7.App.AlertDialog alertDialog;
        Android.Support.V7.App.AlertDialog.Builder alertBuilder;

        public AlertDialogHelper(Activity activity)
        {
            _activity = activity;
        }

        public void ShowDialog()
        {
            alertBuilder = new Android.Support.V7.App.AlertDialog.Builder(_activity);
            alertBuilder.SetView(Resource.Layout.progress_dialog_layout);
            alertBuilder.SetCancelable(false);
            alertDialog = alertBuilder.Show();
        }

        public void CloseDialog()
        {
            if (alertDialog != null)
            {
                alertDialog.Dismiss();
                alertDialog = null;
                alertBuilder = null;
            }
        }
    }
}