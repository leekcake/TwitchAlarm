using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TwitchAlarmShared.Container;

namespace TwitchAlarmShared.Worker
{
    public class StreamerDetector
    {
        public List<StreamerData> Streamers
        {
            get; private set;
        } = new List<StreamerData>();

        public void LoadStreamers()
        {
            Streamers.Clear();
            try
            {
                foreach (var path in PlatformUtils.Instance.GetFiles(Path.Combine(PlatformUtils.Instance.GetConfigBasePath(), "Streamers")))
                {
                    Streamers.Add(StreamerData.LoadFromBytes(PlatformUtils.Instance.ReadAllBytes(path)));
                }
            }
            catch
            {

            }
        }

        public void AddNewStreamer(StreamerData streamer)
        {
            Streamers.Add(streamer);
        }

        public void SaveStreamer(StreamerData streamer)
        {
            if(!Streamers.Contains(streamer))
            {
                throw new Exception("Tried to save streamer data that not managed in detector!");
            }
            var path = Path.Combine(PlatformUtils.Instance.GetConfigBasePath(), "Streamers");
            PlatformUtils.Instance.EnsureDirectory(path);

            path = Path.Combine(path, streamer.Id + ".json");
            PlatformUtils.Instance.WriteAllBytes(path, streamer.SaveToBytes());
        }

        public void RemoveStreamerById(string id)
        {
            Streamers.RemoveAll(t =>
            {
                return t.Id == id;
            });
            try
            {
                PlatformUtils.Instance.DeleteFile(Path.Combine(PlatformUtils.Instance.GetConfigBasePath(), "Streamers", id + ".json"));
            }
            catch
            {

            }
        }
    }
}
