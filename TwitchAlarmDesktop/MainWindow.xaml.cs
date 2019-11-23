using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using TwitchAlarmShared.Container;
using TwitchAlarmShared.Worker;

namespace TwitchAlarmDesktop
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private StreamerDetector detector = new StreamerDetector();
        private StreamerData selectedData;

        private List<StreamerData> filteredData = new List<StreamerData>();

        public MainWindow()
        {
            InitializeComponent();

            detector.LoadStreamers();
            RefreshFilteredList();
        }

        private void StreamerListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!CheckForSelectedItem()) return;

            if( MessageBox.Show($"{selectedData.DisplayName}님을 리스트에서 삭제합니까?", "트위치 알림", MessageBoxButton.YesNo) == MessageBoxResult.No )
            {
                return;
            }
            detector.RemoveStreamerById(selectedData.Id);
            RefreshFilteredList();
        }

        private void StreamerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                selectedData = (StreamerData) StreamerListBox.SelectedItem;
                LoadSelectedIntoUI();
            }
            catch
            {

            }
        }

        private void PreventPopupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!CheckForSelectedItem()) return;
            selectedData.PreventPopup = false;
            detector.SaveStreamer(selectedData);
        }

        private void PreventPopupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!CheckForSelectedItem()) return;
            selectedData.PreventPopup = true;
            detector.SaveStreamer(selectedData);
        }

        private void UseNotifyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!CheckForSelectedItem()) return;
            selectedData.UseNotify = true;
            detector.SaveStreamer(selectedData);
        }

        private void UseNotifyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!CheckForSelectedItem()) return;
            selectedData.UseNotify = false;
            detector.SaveStreamer(selectedData);
        }

        private void NotifySoundRepeatCountLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!CheckForSelectedItem()) return;

            var defValue = selectedData.NotifySoundRepeatCount.ToString();
            if(defValue == "-1")
            {
                defValue = "무한";
            }

            var result = new InputBox("반복 횟수를 정하세요, '무한'으로 입력하면 알림을 끝낼때까지 재생됩니다", "트위치 알람", "").ShowDialog().Trim();
            int value;
            if(result == "무한")
            {
                value = -1;
            }
            else
            {
                try
                {
                    value = int.Parse(result);
                }
                catch
                {
                    MessageBox.Show($"{result}은 올바른 숫자 혹은 '무한'이 아닌걸로 보입니다");
                    return;
                }
            }
            selectedData.NotifySoundRepeatCount = value;
            detector.SaveStreamer(selectedData);
            LoadSelectedIntoUI();
        }

        private void NotifySoundPathLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!CheckForSelectedItem()) return;
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = "*.*";
            dlg.Filter = "Audio File (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == false)
            {
                return;
            }
            selectedData.NotifySoundPath = dlg.FileName;
            detector.SaveStreamer(selectedData);
            LoadSelectedIntoUI();
        }

        private void DisplayNameLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!CheckForSelectedItem()) return;
            selectedData.DisplayName = new InputBox("닉네임을 정하세요", "트위치 알람", selectedData.DisplayName).ShowDialog();
            detector.SaveStreamer(selectedData);
            LoadSelectedIntoUI();
            RefreshFilteredList();
        }

        private void AddStreamerButton_Click(object sender, RoutedEventArgs e)
        {
            var streamer = new StreamerData();
            streamer.Id = new InputBox("아이디를 정하세요", "트위치 알람", "").ShowDialog();
            if(streamer.Id.Trim() == "")
            {
                MessageBox.Show("스트리머 아이디는 공백일 수 없습니다", "트위치 알림");
                return;
            }
            detector.AddNewStreamer(streamer);
            detector.SaveStreamer(streamer);
            RefreshFilteredList();
        }

        private bool CheckForSelectedItem()
        {
            if (selectedData == null)
            {
                MessageBox.Show("먼저 스트리머를 선택해야 합니다");
                return false;
            }
            return true;
        }

        private void LoadSelectedIntoUI()
        {
            Idlabel.Content = $"스트리머 아이디: {selectedData.Id}";
            DisplayNameLabel.Content = $"스트리머 이름: {selectedData.DisplayName}";
            NotifySoundPathLabel.Content = $"알림음 파일: { Path.GetFileName(selectedData.NotifySoundPath) }";
            if (selectedData.NotifySoundRepeatCount == -1)
            {
                NotifySoundRepeatCountLabel.Content = "알림음을 재생할 횟수: 무한";
            }
            else
            {
                NotifySoundRepeatCountLabel.Content = $"알림음을 재생할 횟수: {selectedData.NotifySoundRepeatCount}";
            }
            PreventPopupCheckBox.IsChecked = selectedData.PreventPopup;
            UseNotifyCheckBox.IsChecked = selectedData.UseNotify;
        }

        private void RefreshFilteredList()
        {
            filteredData.Clear();
            foreach(var streamer in detector.Streamers)
            {
                if(FilterStreamerTextBox.Text.Trim() != "" && !streamer.DisplayName.Contains(FilterStreamerTextBox.Text))
                {
                    continue;
                }
                filteredData.Add(streamer);
            }

            StreamerListBox.ItemsSource = null;
            StreamerListBox.ItemsSource = filteredData;
        }
    }
}
