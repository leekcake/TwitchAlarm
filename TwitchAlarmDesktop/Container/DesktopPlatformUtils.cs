using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchAlarmShared.Worker;

namespace TwitchAlarmDesktop.Container
{
    public class DesktopPlatformUtils : PlatformUtils
    {
        public override void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public override void EnsureDirectory(string path)
        {
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }

        public override string GetConfigBasePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
        }

        public override string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }

        public override byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public override void WriteAllBytes(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }
    }
}
