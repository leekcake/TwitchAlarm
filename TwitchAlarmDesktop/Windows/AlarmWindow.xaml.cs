using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TwitchAlarmShared.Container;

namespace TwitchAlarmDesktop.Windows
{
    /// <summary>
    /// AlarmWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AlarmWindow : Window
    {
        private WaveOutEvent device = new WaveOutEvent();
        private AudioFileReader file;

        private int PlaybackCount = 0;
        private StreamerData data;

        public bool IsClosed = false;

        public AlarmWindow()
        {
            InitializeComponent();

            Closed += AlarmWindow_Closed;
        }

        private void AlarmWindow_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
            device.Stop();
            file.Dispose();
            device.Dispose();
        }

        public void InitWithStreamerData(StreamerData data)
        {
            this.data = data;
            file = new AudioFileReader(data.NotifySoundPath);
            device.Init(file);
            device.Play();
            device.PlaybackStopped += Device_PlaybackStopped;

            ContentTextBlock.Text = $"{data.DisplayName}님의 방송이 켜졌습니다!";
        }

        private void Device_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (IsClosed) return;
            if( data.NotifySoundRepeatCount > -1 )
            {
                PlaybackCount++;
                if(data.NotifySoundRepeatCount == PlaybackCount)
                {
                    return;
                }
            }
            file.Seek(0, System.IO.SeekOrigin.Begin);
            device.Play();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
