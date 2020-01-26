using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Firebase.Database;
using Firebase;
// ReSharper disable All

namespace Ctrip.Rider
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        FirebaseDatabase _database;
        Button _btnTestConnection;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            _btnTestConnection = FindViewById<Button>(Resource.Id.myButton);
            _btnTestConnection.Click += BtnTestConnection_Click;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void BtnTestConnection_Click(object sender, EventArgs e)
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var app = FirebaseApp.InitializeApp(this);

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("ctrip-50eab")
                    .SetApiKey("AIzaSyDBk-f9zqpg1uGZYAHUt5kV8xbOxGQiS9w")
                    .SetDatabaseUrl("https://ctrip-50eab.firebaseio.com")
                    .SetStorageBucket("ctrip-50eab.appspot.com")
                    .Build();

                app = FirebaseApp.InitializeApp(this, options);
                _database = FirebaseDatabase.GetInstance(app);
            }
            
            _database = FirebaseDatabase.GetInstance(app);
  
            DatabaseReference databaseReference = _database.GetReference("UserSupport");
            databaseReference.SetValue("Ticket1");

            Toast.MakeText(this, "Completed", ToastLength.Short).Show();
        }
    }
}