using Android.App;
using Android.Views;

namespace Ctrip.Rider.Helpers
{
    public class CookieBarHelper : Java.Lang.Object
    {
        private readonly Activity _activity;

        public CookieBarHelper(Activity activity)
        {
            _activity = activity;
        }

        public void ShowCookieBar(string title, string message)
        {
            Org.Aviran.CookieBar2.CookieBar.Build(_activity)
               .SetTitle(title)
               .SetMessage(message)
               .SetCookiePosition((int)GravityFlags.Bottom)
               .Show();
        }
    }
}