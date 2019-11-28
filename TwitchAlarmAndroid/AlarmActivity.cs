using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TwitchAlarmShared.Container;

using Uri = Android.Net.Uri;

namespace TwitchAlarmAndroid
{
    [Activity(Label = "AlarmActivity")]
    public class AlarmActivity : Activity
    {
        public static StreamerData alarmTarget;

        private MediaPlayer alarmPlayer = new MediaPlayer();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_alarm);
            Window.AddFlags(WindowManagerFlags.KeepScreenOn |
                WindowManagerFlags.DismissKeyguard |
                WindowManagerFlags.ShowWhenLocked |
                WindowManagerFlags.TurnScreenOn);

            Title = GetString(Resource.String.alarm_title).Replace("/NAME/", alarmTarget.DisplayName);

            var textview = FindViewById<TextView>(Resource.Id.titleTextView);
            textview.Text = Title;

            var button = FindViewById<Button>(Resource.Id.ok_button);
            button.Click += Button_Click;

            try
            {
                var builder = new AudioAttributes.Builder();
                builder.SetContentType(AudioContentType.Music);
                builder.SetUsage(AudioUsageKind.Alarm);
                alarmPlayer.SetAudioAttributes(builder.Build());
                alarmPlayer.SetDataSource(ApplicationContext, Uri.Parse(alarmTarget.NotifySoundPath));
                alarmPlayer.Looping = true;
                alarmPlayer.Prepare();
                alarmPlayer.Start();
            }
            catch
            {

            }
        }

        protected override void OnResume()
        {
            try
            {
                alarmPlayer.Start();
            }
            catch
            {

            }
            base.OnResume();
        }

        protected override void OnStop()
        {
            try
            {
                alarmPlayer.Pause();
            }
            catch
            {

            }
            base.OnStop();
        }

        protected override void OnDestroy()
        {
            try
            {
                alarmPlayer.Dispose();
            }
            catch
            {

            }
            base.OnDestroy();
        }

        private void Button_Click(object sender, EventArgs e) => Finish();
    }
}