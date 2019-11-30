using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace TwitchAlarmAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
    public class PermissionActivity : AppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted ||
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted)
            {
                string[] permission = null;
                if(ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted &&
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted)
                {
                    Toast.MakeText(ApplicationContext, GetString(Resource.String.permission_read_storage_description) + "\n" + GetString(Resource.String.permission_internet_description), ToastLength.Long).Show();
                    permission = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.Internet };
                }
                else if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
                {
                    Toast.MakeText(ApplicationContext, Resource.String.permission_read_storage_description, ToastLength.Long).Show();
                    permission = new string[] { Manifest.Permission.ReadExternalStorage };
                }
                else if(ContextCompat.CheckSelfPermission(this, Manifest.Permission.Internet) != (int)Permission.Granted)
                {
                    Toast.MakeText(ApplicationContext, Resource.String.permission_internet_description, ToastLength.Long).Show();
                    permission = new string[] { Manifest.Permission.Internet };
                }
                new Task(async () =>
                {
                    await Task.Delay(2000);
                    RunOnUiThread(() =>
                    {
                        ActivityCompat.RequestPermissions(this, permission, 1);
                    });
                }).Start();
            }
            else
            {
                StartActivity(new Intent(ApplicationContext, typeof(MainActivity)));
                Finish();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            for(int i = 0; i < permissions.Length; i++)
            {
                if(grantResults[i] == Permission.Denied)
                {
                    switch(permissions[i])
                    {
                        case Manifest.Permission.ReadExternalStorage:
                            Toast.MakeText(ApplicationContext, Resource.String.permission_read_storage_description, ToastLength.Long).Show();
                            break;
                        case Manifest.Permission.Internet:
                            Toast.MakeText(ApplicationContext, Resource.String.permission_internet_description, ToastLength.Long).Show();
                            break;
                    }
                    Finish();
                    return;
                }
            }
            StartActivity(new Intent(ApplicationContext, typeof(MainActivity)));
            Finish();
        }
    }
}