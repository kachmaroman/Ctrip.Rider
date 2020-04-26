using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using System;

namespace Ctrip.Rider.Helpers
{
    public class KeyboardHelper: Java.Lang.Object
    {
        public static void HideKeyboard(Activity activity)
        {
            try
            {
                InputMethodManager inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(activity.CurrentFocus?.ApplicationWindowToken, 0);
            }
            catch(Exception e)
            {
                Toast.MakeText(activity, e.Message, ToastLength.Short).Show();
            }
        }
    }
}