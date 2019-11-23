﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchAlarmShared.Container
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StreamerData
    {
        /// <summary>
        /// 스트리머의 아이디 (lilac_unicorn_)
        /// </summary>
        [JsonProperty]
        public string Id { get; set; }
        /// <summary>
        /// 스트리머의 표시 이름 (유닉혼)
        /// </summary>
        [JsonProperty]
        public string DisplayName { get; set; }
        /// <summary>
        /// 이 스트리머의 알림을 활성화 할지의 여부
        /// </summary>
        [JsonProperty]
        public bool UseNotify { get; set; } = true;
        /// <summary>
        /// 이 알림이 창을 띄우면서 방송중임을 알릴지의 여부
        /// </summary>
        [JsonProperty]
        public bool PreventPopup { get; set; } = false;
        /// <summary>
        /// 알림 소리를 위한 파일의 위치
        /// </summary>
        [JsonProperty]
        public string NotifySoundPath { get; set; }
        /// <summary>
        /// 알림 소리를 반복할 횟수
        /// </summary>
        [JsonProperty]
        public int NotifySoundRepeatCount { get; set; } = -1;

        /// <summary>
        /// 이 스트리머가 방송중인지의 여부
        /// </summary>
        public bool IsInBroadcasting = false;

        
        public string IsInBroadcastingString
        {
            get
            {
                if (IsInBroadcasting) return " (방송중)";
                return "";
            }
        }

        public byte[] SaveToBytes()
        {
            var str = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(str);
        }

        public static StreamerData LoadFromBytes(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<StreamerData>(str);
        }

        public override string ToString()
        {
            if(DisplayName == null || DisplayName == "")
            {
                return $"@{Id}{IsInBroadcastingString}";
            }
            return $"{DisplayName}{IsInBroadcastingString}";
        }
    }
}