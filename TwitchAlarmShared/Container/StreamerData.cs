using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchAlarmShared.Container
{
    public class StreamerData
    {
        /// <summary>
        /// 스트리머의 아이디 (lilac_unicorn_)
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 스트리머의 표시 이름 (유닉혼)
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 이 스트리머의 알림을 활성화 할지의 여부
        /// </summary>
        public bool UseNotify { get; set; }
        /// <summary>
        /// 이 알림이 창을 띄우면서 방송중임을 알릴지의 여부
        /// </summary>
        public bool PreventPopup { get; set; }
        /// <summary>
        /// 알림 소리를 위한 파일의 위치
        /// </summary>
        public string NotifySoundPath { get; set; }
        /// <summary>
        /// 알림 소리를 반복할 횟수
        /// </summary>
        public int NotifySoundRepeatCount { get; set; }
    }
}
