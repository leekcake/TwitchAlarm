using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using TwitchAlarmShared.Container;
using TwitchLib.Api;
using TwitchLib.Client.Models;

namespace TwitchAlarmShared.Worker
{
    public class StreamerDetector
    {
        public interface Listener
        {
            void OnBroadcastStartup(StreamerData data);
        }

        public Listener StartListener;

        public List<StreamerData> Streamers
        {
            get; private set;
        } = new List<StreamerData>();

        public TwitchAPI Twitch { get; private set; } = new TwitchAPI();

        public bool TokenNotReady
        {
            get
            {
                return AccessToken == null || RefreshToken == null;
            }
        }

        public string AccessToken
        {
            get; private set;
        }

        public string RefreshToken
        {
            get; private set;
        }

        public void TryToReadToken()
        {
            try
            {
                var reader = new BinaryReader(new MemoryStream(PlatformUtils.Instance.ReadAllBytes(Path.Combine(PlatformUtils.Instance.GetConfigBasePath(), "token.id"))));
                AccessToken = reader.ReadString();
                RefreshToken = reader.ReadString();

                reader.Close();
                AssignSettings();
            }
            catch
            {

            }
        }

        public void SetToken(TwitchAuthResponse response)
        {
            AccessToken = response.access_token;
            RefreshToken = response.refresh_token;

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(AccessToken);
            writer.Write(RefreshToken);
            writer.Close();

            PlatformUtils.Instance.WriteAllBytes(Path.Combine(PlatformUtils.Instance.GetConfigBasePath(), "token.id"), stream.ToArray());
            AssignSettings();
        }

        public void AssignSettings()
        {
            Twitch.Settings.ClientId = TwitchID.ID;
            Twitch.Settings.Secret = TwitchID.SECRET_ID;
            Twitch.Settings.AccessToken = AccessToken;
        }

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
            if(Streamers.Count >= 100)
            {
                throw new Exception("Too many streamer :(");
            }
        }

        public void SaveStreamer(StreamerData streamer)
        {
            if (!Streamers.Contains(streamer))
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

        public async Task Check(bool fire)
        {
            if (Streamers.Count == 0) return;
            try
            {
                var response = await Twitch.Helix.Streams.GetStreamsAsync(userLogins: new List<string>(Streamers.Select(t => t.Id)));
                foreach (var stream in response.Streams)
                {
                    var data = Streamers.Find(t => t.InternalId == stream.UserId);
                    data.IsLiveDetectedOnTick = true;
                    if(!data.IsInBroadcasting)
                    {
                        data.IsInBroadcasting = true;
                        if(fire)
                            StartListener?.OnBroadcastStartup(data);
                    }
                }

                foreach(var streamer in Streamers)
                {
                    if(streamer.IsLiveDetectedOnTick)
                    {
                        streamer.IsLiveDetectedOnTick = false;
                    }
                    else
                    {
                        if(streamer.IsInBroadcasting)
                        {
                            streamer.IsInBroadcasting = false;
                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}
