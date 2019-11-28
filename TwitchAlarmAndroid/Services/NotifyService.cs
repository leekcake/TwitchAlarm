using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using TwitchAlarmShared.Container;
using TwitchAlarmShared.Worker;

namespace TwitchAlarmAndroid.Services
{
    [Service]
    public class NotifyService : Service, StreamerDetector.Listener
    {
        public static StreamerDetector Detector
        {
            get; private set;
        }

        public static Action OnServiceStart;
        public static Action OnCheckPerformed;
        public static Action<int> OnTimeChanged;
        public static int RefreshTime { get; private set; } = -1;
        private bool IsAlive = true;

        Handler handler;

        public static void MakeDetector()
        {
            Detector = new StreamerDetector();
        }

        void StartForegroundService()
        {
            Intent notificationIntent = new Intent(this, typeof(MainActivity));
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, 0);

            //RemoteViews remoteViews = new RemoteViews(PackageName, Resource.Layout.notification_service);

            NotificationCompat.Builder builder;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                String CHANNEL_ID = "twitch_alarm_channel";
                NotificationChannel channel = new NotificationChannel(CHANNEL_ID,
                    "Twitch Alarm Channel",
                    NotificationImportance.Default);

                ((NotificationManager)GetSystemService(Context.NotificationService))
                  .CreateNotificationChannel(channel);

                builder = new NotificationCompat.Builder(this, CHANNEL_ID);
            }
            else
            {
                builder = new NotificationCompat.Builder(this);
            }
            builder.SetSmallIcon(Resource.Mipmap.ic_launcher);
            builder.SetContentTitle(GetString(Resource.String.notify_service_title));
            builder.SetContentText(GetString(Resource.String.notify_service_text));
            builder.SetContentIntent(pendingIntent);

            StartForeground(1, builder.Build());
        }

        public override void OnCreate()
        {
            StartForegroundService();
            handler = new Handler();
            new Task(() =>
            {
                Detector = new StreamerDetector();
                Detector.TryToReadToken();
                if (Detector.TokenNotReady)
                {
                    TwitchSelfServer self = new TwitchSelfServer();
                    self.Start();

                    handler.Post(() =>
                    {
                        string url = String.Format("https://id.twitch.tv/oauth2/authorize?response_type=code&client_id={0}&redirect_uri=http://localhost:8080/twitch/callback&state=123456", TwitchID.ID);
                        var uri = Android.Net.Uri.Parse(url);
                        var intent = new Intent(Intent.ActionView, uri);
                        intent.AddFlags(ActivityFlags.NewTask);
                        StartActivity(intent);
                    });

                    while (self.ReceivedResponse == null)
                    {
                        Thread.Sleep(1000);
                    }

                    Detector.SetToken(self.ReceivedResponse);
                }
                Detector.StartListener = this;
                Detector.LoadStreamers();
                OnServiceStart?.Invoke();
            }).Start();
            Repeat();
            base.OnCreate();
        }

        public async Task Repeat()
        {
            bool fire = false;
            while (IsAlive)
            {
                if (Detector == null)
                {
                    await Task.Delay(1000);
                    continue;
                }
                await Detector.Check(fire);
                OnCheckPerformed?.Invoke();
                for (int i = 0; i < 30; i++)
                {
                    RefreshTime = 30 - i;
                    OnTimeChanged?.Invoke(RefreshTime);
                    await Task.Delay(1000);
                }
                fire = true;
            }
        }

        public override void OnDestroy()
        {
            IsAlive = false;
            base.OnDestroy();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public void OnBroadcastStartup(StreamerData data)
        {

        }
    }
}