using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TwitchAlarmDesktop.Container;
using TwitchAlarmShared.Worker;

namespace TwitchAlarmDesktop
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            PlatformUtils.Instance = new DesktopPlatformUtils();

            base.OnStartup(e);
        }
    }
}
