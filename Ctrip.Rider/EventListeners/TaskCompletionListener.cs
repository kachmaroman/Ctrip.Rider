using System;
using Android.Gms.Tasks;

namespace Ctrip.Rider.EventListeners
{
    public class TaskCompletionListener : Java.Lang.Object, IOnSuccessListener, IOnFailureListener
    {
        public event EventHandler Success;
        public event EventHandler Failure;

        public void OnFailure(Java.Lang.Exception e)
        {
            Failure?.Invoke(this, new EventArgs());
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            Success?.Invoke(this, new EventArgs());
        }
    }
}