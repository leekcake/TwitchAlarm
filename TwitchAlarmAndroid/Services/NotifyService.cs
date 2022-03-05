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
        public static Func<bool> AskAuthHandler;
        public static Action OnCheckPerformed;
        public static Action<int> OnTimeChanged;

        public const int TIME_IN_REFRESH = 0;
        public const int TIME_WAIT_FOR_TOKEN = -1;
        public const int TIME_WAIT_FOR_STREAMER = -2;
        public const int TIME_NO_ACTIVE_STREAMER = -3;

        private bool isStreamerListReaded = false;

        public static int RefreshTime { get; private set; } = -1;
        private bool IsAlive = true;

        private NotificationCompat.Builder notifyBuilder;
        private NotificationManager notificationManager;

        Handler handler;

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
                    NotificationImportance.Min);
                channel.SetShowBadge(false);
                channel.SetSound(null, null);

                ((NotificationManager)GetSystemService(Context.NotificationService))
                  .CreateNotificationChannel(channel);

                builder = new NotificationCompat.Builder(this, CHANNEL_ID);
            }
            else
            {
                builder = new NotificationCompat.Builder(this);
                builder.SetPriority((int) NotificationPriority.Min);
            }
            builder.SetOnlyAlertOnce(true);
            builder.SetSmallIcon(Resource.Mipmap.ic_launcher);
            builder.SetContentTitle(GetString(Resource.String.notify_service_title));
            builder.SetContentText(GetString(Resource.String.wait_read_streamer_list));;
            builder.SetContentIntent(pendingIntent);

            notifyBuilder = builder;

            StartForeground(1, builder.Build());
        }

        private void UpdateNotify(string message, bool useChronometer = false, bool useHandler = false)
        {
            Action action = new Action(() =>
            {
                notifyBuilder.SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis());
                notifyBuilder.SetContentText(message);
                notifyBuilder.SetUsesChronometer(useChronometer);
                notificationManager.Notify(1, notifyBuilder.Build());
            });
            if(useHandler)
            {
                handler.Post(action);
            }
            else
            {
                action.Invoke();
            }
        }

        public override void OnCreate()
        {
            notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            StartForegroundService();
            handler = new Handler();
            new Task(() =>
            {
                Detector = new StreamerDetector();
                Detector.LoadStreamers();
                isStreamerListReaded = true;
                Detector.TryToReadToken();
                if (Detector.TokenNotReady)
                {
                    if(!AskAuthHandler())
                    {
                        IsAlive = false;
                        StopSelf();
                        return;
                    }
                    TwitchSelfServer self = new TwitchSelfServer();
                    self.Start();

                    handler.Post(() =>
                    {
                        UpdateNotify(GetString(Resource.String.wait_token));
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
                handler.Post(() =>
                {
                    notifyBuilder.SetContentText(GetString(Resource.String.wait_for_fetcher));
                    notificationManager.Notify(1, notifyBuilder.Build());
                });
                Detector.StartListener = this;
                OnServiceStart?.Invoke();
                
            }).Start();
            Repeat();
            base.OnCreate();
        }

        public async Task Repeat()
        {
            bool fire = false;
            var pm = (PowerManager) GetSystemService(PowerService);
            var wakelock = pm.NewWakeLock(WakeLockFlags.Partial, "TwitchAlarm");
            wakelock.Acquire();
            while (IsAlive)
            {
                if (Detector == null)
                {
                    await Task.Delay(1000);
                    continue;
                }
                if (Detector.TokenNotReady || !isStreamerListReaded)
                {
                    if(Detector.TokenNotReady)
                    {
                        OnTimeChanged?.Invoke(TIME_WAIT_FOR_TOKEN);
                    }
                    else
                    {
                        OnTimeChanged?.Invoke(TIME_WAIT_FOR_STREAMER);
                    }
                    await Task.Delay(1000);
                    continue;
                }
                var count = Detector.GetUseNotifyTotal();
                if (count == 0)
                {
                    UpdateNotify(GetString(Resource.String.no_active_streamer), useHandler: true);
                    OnTimeChanged?.Invoke(TIME_NO_ACTIVE_STREAMER);
                    await Task.Delay(1000);
                    continue;
                }
                OnTimeChanged?.Invoke(TIME_IN_REFRESH);
                UpdateNotify(GetString(Resource.String.left_second_for_refresh_in_refresh), useHandler: true);
                await Detector.Check(fire);
                UpdateNotify(GetString(Resource.String.notify_service_text).Replace("/COUNT/", count + ""), useChronometer: true, useHandler: true);
                OnCheckPerformed?.Invoke();
                for (int i = 0; i < 30; i++)
                {
                    if(!IsAlive)
                    {
                        break;
                    }
                    RefreshTime = 30 - i;
                    OnTimeChanged?.Invoke(RefreshTime);
                    await Task.Delay(1000);
                }
                fire = true;
            }
            wakelock.Release();
        }

        public override void OnDestroy()
        {
            Detector = null;
            IsAlive = false;
            base.OnDestroy();
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public void OnBroadcastStartup(StreamerData data)
        {
            handler.Post(() =>
            {
                AlarmActivity.alarmTarget = data;
                var intent = new Intent(this, typeof(AlarmActivity));
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
            });
        }
    }
}