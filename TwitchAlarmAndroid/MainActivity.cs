using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using TwitchAlarmAndroid.Container;
using TwitchAlarmAndroid.Services;
using TwitchAlarmShared.Container;
using TwitchAlarmShared.Worker;

namespace TwitchAlarmAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
    public class MainActivity : AppCompatActivity
    {
        private ListView streamerListView;
        private ArrayAdapter streamerListAdapter;

        private TextView leftTimeTextView;

        private Button stopButton;
        private Button helpButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            var util = new AndroidPlatformUtils(ApplicationContext);
            PlatformUtils.Instance = util;

            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabOnClick;

            streamerListView = FindViewById<ListView>(Resource.Id.streamerListView);
            leftTimeTextView = FindViewById<TextView>(Resource.Id.leftTimeTextView);
            streamerListView.ItemClick += StreamerListView_ItemClick;
            streamerListView.ItemLongClick += StreamerListView_ItemLongClick;

            stopButton = FindViewById<Button>(Resource.Id.stop_button);
            helpButton = FindViewById<Button>(Resource.Id.help_button);

            stopButton.Click += StopButton_Click;
            helpButton.Click += HelpButton_Click;
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(ApplicationContext, typeof(HelpActivity)));
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StopService(new Intent(ApplicationContext, typeof(NotifyService)));
            Finish();
        }

        private void StreamerListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var streamerData = NotifyService.Detector.Streamers[e.Position];
            var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.app_name);
            builder.SetMessage(GetString(Resource.String.ask_for_delete).Replace("/NAME/", streamerData.DisplayName));
            builder.SetPositiveButton(Resource.String.yes, (ev, ct) =>
            {
                NotifyService.Detector.RemoveStreamerById(streamerData.Id);
                streamerListAdapter = new ArrayAdapter<StreamerData>(this, Android.Resource.Layout.SimpleListItem1, NotifyService.Detector.Streamers);
                streamerListView.Adapter = streamerListAdapter;
            });
            builder.SetNegativeButton(Resource.String.no, (ev, ct) =>
            {

            });
            builder.Show();
        }

        protected override void OnStart()
        {
            NotifyService.OnServiceStart = new Action(() =>
            {
                RunOnUiThread(() =>
                {
                    streamerListAdapter = new ArrayAdapter<StreamerData>(this, Android.Resource.Layout.SimpleListItem1, NotifyService.Detector.Streamers);
                    streamerListView.Adapter = streamerListAdapter;
                });
            });

            NotifyService.OnCheckPerformed = new Action(() =>
            {
                RunOnUiThread(() =>
                {
                    if (streamerListAdapter != null)
                    {
                        streamerListAdapter.NotifyDataSetInvalidated();
                    }
                });
            });

            var leftStr = GetString(Resource.String.left_second_for_refresh);
            NotifyService.OnTimeChanged = new Action<int>((time) =>
            {
                RunOnUiThread(() =>
                {
                    if (leftTimeTextView != null)
                        leftTimeTextView.Text = leftStr.Replace("/TIME/", time.ToString());
                });
            });

            NotifyService.AskAuthHandler += new Func<bool>(() =>
            {
                bool? result = null;
                RunOnUiThread(() =>
                {
                    var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                    builder.SetTitle(Resource.String.app_name);
                    builder.SetMessage(Resource.String.ask_for_twitch_login);
                    builder.SetPositiveButton(Resource.String.yes, (ev, ct) =>
                    {
                        result = true;
                    });
                    builder.SetNegativeButton(Resource.String.no, (ev, ct) =>
                    {
                        result = false;
                        Finish();
                    });
                    builder.SetCancelable(false);
                    builder.Show();
                });
                while (!result.HasValue)
                {

                }
                return result.Value;
            });

            StartService(new Android.Content.Intent(this, typeof(NotifyService)));
            base.OnStart();
        }

        protected override void OnStop()
        {
            NotifyService.AskAuthHandler = null;
            NotifyService.OnTimeChanged = null;
            NotifyService.OnCheckPerformed = null;
            NotifyService.OnServiceStart = null;
            base.OnStop();
        }

        private void StreamerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            EditActivity.streamerData = NotifyService.Detector.Streamers[e.Position];
            StartActivity(new Intent(ApplicationContext, typeof(EditActivity)));
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return false;
        }

        protected override void OnResume()
        {
            try
            {
                streamerListAdapter = new ArrayAdapter<StreamerData>(this, Android.Resource.Layout.SimpleListItem1, NotifyService.Detector.Streamers);
                streamerListView.Adapter = streamerListAdapter;
            }
            catch
            {

            }
            base.OnResume();
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            /*
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
                */
            EditActivity.streamerData = null;
            StartActivity(new Intent(ApplicationContext, typeof(EditActivity)));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        { 
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
	}
}

