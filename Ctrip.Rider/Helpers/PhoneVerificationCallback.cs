using Android.Widget;
using Ctrip.Rider.Activities;
using Firebase;
using Firebase.Auth;
using static Firebase.Auth.PhoneAuthProvider;

namespace Ctrip.Rider.Helpers
{
    public class PhoneVerificationCallback : OnVerificationStateChangedCallbacks
    {
        readonly PhoneValidationActivity _instance;
        public string smsCode = string.Empty;

        public PhoneVerificationCallback(PhoneValidationActivity instance)
        {
            _instance = instance;
        }

        public override void OnCodeSent(string verificationId, ForceResendingToken forceResendingToken)
        {
            base.OnCodeSent(verificationId, forceResendingToken);
            
            if (!string.IsNullOrWhiteSpace(_instance.verificationId))
            {
                _instance.verificationId = string.Empty;
                _instance.verificationId = verificationId;
            }
            else
            {
                _instance.verificationId = verificationId;
            }
        }

        public override void OnVerificationCompleted(PhoneAuthCredential credential)
        {
            string strCode = credential.SmsCode;

            if (strCode != null)
            {
                _instance.codePinView.Value = strCode;
                _instance.VerifyCode(strCode);
                _instance.ShowProgressDialog();
            }
        }

        public override void OnVerificationFailed(FirebaseException exception)
        {
            _instance.CloseProgressDialog();
            Toast.MakeText(_instance.ApplicationContext, exception.Message, ToastLength.Long).Show();
        }
    }
}