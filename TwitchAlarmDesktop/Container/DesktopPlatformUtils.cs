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
        public override string GetConfigBasePath()
        {
            return "config";
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
