using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchAlarmShared.Worker
{
    public abstract class PlatformUtils
    {
        public static PlatformUtils Instance { get; set; }

        public abstract void WriteAllBytes(string path, byte[] data);

        public abstract byte[] ReadAllBytes(string path);

        public abstract string GetConfigBasePath();
    }
}
