using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using TwitchAlarmShared.Worker;

using Directory = System.IO.Directory;
using Path = System.IO.Path;
using DotNetFile = System.IO.File;
using Java.Nio.FileNio;

namespace TwitchAlarmAndroid.Container
{
    public class AndroidPlatformUtils : PlatformUtils
    {
        private Context context;
        public AndroidPlatformUtils(Context context)
        {
            this.context = context;
        }

        public override void DeleteFile(string path)
        {
            new File(path).Delete();
        }

        public override void EnsureDirectory(string path)
        {
            new File(path).Mkdirs();
        }

        public override string GetConfigBasePath()
        {
            return context.FilesDir.AbsolutePath;
        }

        public override string[] GetFiles(string path)
        {
            return new File(path).List();
        }

        public override byte[] ReadAllBytes(string path)
        {
            var file = new File(path);
            if(!file.Exists())
            {
                return null;
            }

            return Files.ReadAllBytes(file.ToPath());
        }

        public override void WriteAllBytes(string path, byte[] data)
        {
            var file = new File(path);
            Files.Write(file.ToPath(), data);
        }
    }
}