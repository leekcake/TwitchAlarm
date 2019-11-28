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
    [Activity(Label = "AlarmActivity", LaunchMode =Android.Content.PM.LaunchMode.SingleTop)]
    public class AlarmActivity : Activity
    {
        public static StreamerData alarmTarget;

        private MediaPlayer alarmPlayer = new MediaPlayer();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = GetString(Resource.String.alarm_title).Replace("/NAME/", alarmTarget.DisplayName);

            var button = new Button(this);
            button.Text = GetString(Resource.String.ok);
            button.Click += Button_Click;
            SetContentView(button);

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

        protected override void OnStop()
        {
            try
            {
                alarmPlayer.Stop();
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