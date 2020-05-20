using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TwitchAlarmAndroid.Services;
using TwitchAlarmShared.Container;
using static Android.Views.View;

using System;

using Uri = Android.Net.Uri;
using Android.Database;
using System.Threading.Tasks;
using Android.Support.V4.Content;
using Java.IO;
using Android.Util;
using TwitchAlarmShared.Worker;

using Stream = System.IO.Stream;
using TwitchAlarmAndroid.Container;
using System.IO;

namespace TwitchAlarmAndroid
{
    [Activity(Label = "EditActivity", LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
    public class EditActivity : Activity
    {
        private const int AUDIO_REQUEST_CODE = 1;

        public static StreamerData streamerData;
        private bool isAdd = false;

        private EditText idEditText;
        private EditText nameEditText;
        private EditText notifyRepeatCountEditText;

        private Button selectAudioFileButton;
        private Button previewAlarmAudioButton;
        private Button saveButton;
        private Button previewAlarmButton;

        private CheckBox useNotifyCheckBox;
        private CheckBox preventPopupCheckBox;

        private MediaPlayer previewPlayer = new MediaPlayer();

        /// <summary>
        /// 실제 스트리머를 베이스로 한 아이디 샘플
        /// 이 리스트는 새 항목이 추가될때마다 랜덤하게 섞어야 합니다.
        /// </summary>
        private static readonly string[] ID_AND_NAME_SAMPLES = new string[] {
            "뽀큐단의 주인공 뽀카링", "pocari_on",
            "견자희 스튜디오", "wkgml",
            "잔잔한 이렛_", "layered20",
            "연퐁이네 행복마을 이장 이연순", "e_yeon",
            "탄빵베이커리 주인 빵룽", "roong__",
            "다도방 주인 도화님", "dohwa_0904",
            "무지개 나라속 유닉혼", "lilac_unicorn_",
            "헨타 이초홍씨", "h920103",
            "정육점 주인 츄즈미_", "chplease",
            "豆묘송이", "myosonge",
            "새장속 노페토리", "nopetori",
            "뿅뿅단을 이끄는 뿅아가", "rupin074"
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_edit);

            idEditText = FindViewById<EditText>(Resource.Id.streamerIdEditText);
            nameEditText = FindViewById<EditText>(Resource.Id.streamerNameEditText);

            var random = (new Random().Next(ID_AND_NAME_SAMPLES.Length / 2) * 2) - 2;
            if(random < 0)
            {
                random = 0;
            }
            idEditText.Hint = ID_AND_NAME_SAMPLES[random + 1];
            nameEditText.Hint = ID_AND_NAME_SAMPLES[random];

            notifyRepeatCountEditText = FindViewById<EditText>(Resource.Id.streamerNotifyRepeatCountEditText);

            selectAudioFileButton = FindViewById<Button>(Resource.Id.selectAudioFileButton);
            previewAlarmAudioButton = FindViewById<Button>(Resource.Id.previewAudioFileButton);
            saveButton = FindViewById<Button>(Resource.Id.saveButton);
            previewAlarmButton = FindViewById<Button>(Resource.Id.previewAlarmButton);

            useNotifyCheckBox = FindViewById<CheckBox>(Resource.Id.streamerUseNotifyCheckBox);
            preventPopupCheckBox = FindViewById<CheckBox>(Resource.Id.streamerPreventPopupCheckBox);

            if (streamerData == null)
            {
                isAdd = true;
                streamerData = new StreamerData();
                Title = GetString(Resource.String.add_title);
            }
            else
            {
                idEditText.Enabled = false;
                Title = GetString(Resource.String.edit_title);
            }
            idEditText.Text = streamerData.Id;
            nameEditText.Text = streamerData.DisplayName;
            if (streamerData.NotifySoundRepeatCount == -1)
            {
                notifyRepeatCountEditText.Text = "무한";
            }
            else
            {
                notifyRepeatCountEditText.Text = streamerData.NotifySoundRepeatCount.ToString();
            }

            useNotifyCheckBox.Checked = streamerData.UseNotify;
            preventPopupCheckBox.Checked = streamerData.PreventPopup;

            selectAudioFileButton.Click += SelectAudioFileButton_Click;
            previewAlarmAudioButton.Click += PreviewAlarmAudioButton_Click;
            saveButton.Click += SaveButton_Click;
            previewAlarmButton.Click += PreviewAlarmButton_Click;

            useNotifyCheckBox.CheckedChange += UseNotifyCheckBox_CheckedChange;
        }

        private void PreviewAlarmButton_Click(object sender, EventArgs e)
        {
            AlarmActivity.alarmTarget = streamerData;
            StartActivity(new Intent(this, typeof(AlarmActivity)));
        }

        protected override void OnStop()
        {
            try
            {
                previewPlayer.Stop();
            }
            catch
            {

            }
            base.OnStop();
        }

        protected override void OnDestroy()
        {
            streamerData = null;
            try
            {
                previewPlayer.Dispose();
            }
            catch
            {

            }

            base.OnDestroy();
        }

        private bool UIToData()
        {
            if(idEditText.Text.Trim() == "")
            {
                Toast.MakeText(ApplicationContext, Resource.String.empty_streamer_id, ToastLength.Long).Show();
                return false;
            } 

            streamerData.Id = idEditText.Text;
            streamerData.DisplayName = nameEditText.Text;
            if (notifyRepeatCountEditText.Text == "무한")
            {
                streamerData.NotifySoundRepeatCount = -1;
            }
            else
            {
                try
                {
                    streamerData.NotifySoundRepeatCount = int.Parse(notifyRepeatCountEditText.Text);
                }
                catch
                {
                    Toast.MakeText(ApplicationContext, GetString(Resource.String.bad_repeat_count).Replace("/COUNT/", notifyRepeatCountEditText.Text), ToastLength.Long).Show();
                    return false;
                }
            }
            streamerData.UseNotify = useNotifyCheckBox.Checked;
            streamerData.PreventPopup = preventPopupCheckBox.Checked;
            return true;
        }

        private bool Save()
        {
            try
            {
                streamerData.InternalId = NotifyService.Detector.Twitch.V5.Users.GetUserByNameAsync(streamerData.Id).Result.Matches[0].Id;
            }
            catch
            {
                return false;
            }
            if (isAdd)
            {
                NotifyService.Detector.AddNewStreamer(streamerData);
            }
            NotifyService.Detector.SaveStreamer(streamerData);
            return true;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if(!UIToData())
            {
                return;
            }
            new Task(() =>
            {
                var result = Save();
                RunOnUiThread(() =>
                {
                    if(!result)
                    {
                        Toast.MakeText(ApplicationContext, Resource.String.bad_streamer_id, ToastLength.Short).Show();
                    }
                    else
                    {
                        Finish();
                    }
                });
            }).Start();
        }

        private void UseNotifyCheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked && !NotifyService.Detector.CheckNotifyLimit())
            {
                Toast.MakeText(ApplicationContext, Resource.String.notify_limit_exceeded, ToastLength.Short).Show();
                useNotifyCheckBox.Checked = false;
                return;
            }
        }

        private void PreviewAlarmAudioButton_Click(object sender, EventArgs e)
        {
            if (streamerData.NotifySoundPath == null)
            {
                Toast.MakeText(ApplicationContext, Resource.String.no_sound_on_play_notify_sound, ToastLength.Short).Show();
                return;
            }
            try
            {
                if (previewPlayer.IsPlaying)
                {
                    previewPlayer.Stop();
                }
                else
                {
                    previewPlayer.Reset();
                    var builder = new AudioAttributes.Builder();
                    builder.SetContentType(AudioContentType.Music);
                    builder.SetUsage(AudioUsageKind.Alarm);
                    previewPlayer.SetAudioAttributes(builder.Build());
                    previewPlayer.SetDataSource(streamerData.NotifySoundPath); //SetDataSource(ApplicationContext, Uri.Parse(streamerData.NotifySoundPath));
                    previewPlayer.Prepare();
                    previewPlayer.Start();
                }
            }
            catch
            {
                Toast.MakeText(ApplicationContext, Resource.String.exception_on_play_notify_sound, ToastLength.Short).Show();
            }
        }

        private void SelectAudioFileButton_Click(object sender, EventArgs e)
        {
            if(isAdd)
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle(Resource.String.app_name);
                builder.SetMessage(Resource.String.must_save_before_set_notify_sound);
                builder.SetPositiveButton(Resource.String.yes, (ev, ct) =>
                {
                    if(!UIToData())
                    {
                        return;
                    }                    
                    new Task(() =>
                    {
                        var result = Save();
                        RunOnUiThread(() =>
                        {
                            if(!result)
                            {
                                Toast.MakeText(ApplicationContext, Resource.String.bad_streamer_id, ToastLength.Short).Show();
                                return;
                            }
                            isAdd = false;
                            idEditText.Enabled = false;
                            Title = GetString(Resource.String.edit_title);

                            var selfIntent = new Intent();
                            selfIntent.SetType("audio/*");
                            selfIntent.SetAction(Intent.ActionGetContent);
                            StartActivityForResult(selfIntent, AUDIO_REQUEST_CODE);
                        });
                    }).Start();
                });
                builder.SetNegativeButton(Resource.String.no, (ev, ct) =>
                {

                });
                builder.Show();
                return;
            }
            var intent = new Intent();
            intent.SetType("audio/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(intent, AUDIO_REQUEST_CODE);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == AUDIO_REQUEST_CODE)
            {
                if (resultCode == Result.Ok)
                {
                    SetNotifySoundPathWithUri(data.Data);
                    previewPlayer.Stop();
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void SetNotifySoundPathWithUri(Android.Net.Uri uri)
        {
            bool isKitKat = Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat;

            if (isKitKat && DocumentsContract.IsDocumentUri(this, uri))
            {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    string[] split = docId.Split(chars);
                    string type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        streamerData.NotifySoundPath = Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                        return;
                    }
                }
                // DownloadsProvider
                else if (isDownloadsDocument(uri))
                {
                    var id = DocumentsContract.GetDocumentId(uri);

                    if (id != null && id.StartsWith("raw:"))
                    {
                        streamerData.NotifySoundPath = id.Substring(4);
                        return;
                    }

                    String[] contentUriPrefixesToTry = new String[]{
                        "content://downloads/public_downloads",
                        "content://downloads/my_downloads",
                        "content://downloads/all_downloads"
                    };

                    foreach (String contentUriPrefix in contentUriPrefixesToTry)
                    {
                        Uri contentUri = ContentUris.WithAppendedId(Uri.Parse(contentUriPrefix), long.Parse(id));
                        try
                        {
                            String path = getDataColumn(this, contentUri, null, null);
                            if (path != null)
                            {
                                streamerData.NotifySoundPath = path;
                                return;
                            }
                        }
                        catch (Exception e) { }
                    }

                    var builder = new AlertDialog.Builder(this);
                    builder.SetTitle(Resource.String.app_name);
                    builder.SetMessage(Resource.String.need_to_copy_to_local);
                    builder.SetPositiveButton(Resource.String.yes, (ev, ct) =>
                    {
                        try
                        {
                            Stream stream = ContentResolver.OpenInputStream(uri);
                            var data = AndroidPlatformUtils.ReadFully(stream);
                            stream.Close();
                            var notify = Path.Combine(PlatformUtils.Instance.GetConfigBasePath(), "notify");
                            PlatformUtils.Instance.EnsureDirectory(notify);
                            streamerData.NotifySoundPath = Path.Combine(notify, streamerData.Id);
                            PlatformUtils.Instance.WriteAllBytes(streamerData.NotifySoundPath, data);
                        }
                        catch
                        {
                            streamerData.NotifySoundPath = null;
                        }
                    });
                    builder.SetNegativeButton(Resource.String.no, (ev, ct) =>
                    {

                    });
                    builder.Show();
                    return;
                }
                // MediaProvider
                else if (isMediaDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    String[] split = docId.Split(chars);

                    String type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    String selection = "_id=?";
                    String[] selectionArgs = new String[]
                    {
                split[1]
                    };

                    streamerData.NotifySoundPath = getDataColumn(this, contentUri, selection, selectionArgs);
                    return;
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {

                // Return the remote address
                if (isGooglePhotosUri(uri))
                {
                    streamerData.NotifySoundPath = uri.LastPathSegment;
                    return;
                }
                streamerData.NotifySoundPath = getDataColumn(this, uri, null, null);
                return;
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                streamerData.NotifySoundPath = uri.Path;
                return;
            }
        }

        public static String getDataColumn(Context context, Android.Net.Uri uri, String selection, String[] selectionArgs)
        {
            ICursor cursor = null;
            String column = "_data";
            String[] projection =
            {
        column
    };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        //Whether the Uri authority is ExternalStorageProvider.
        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is DownloadsProvider.
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is MediaProvider.
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is Google Photos.
        public static bool isGooglePhotosUri(Android.Net.Uri uri)
        {
            return "com.google.android.apps.photos.content".Equals(uri.Authority);
        }
    }
}