using System.Collections.Generic;
using System.Web.Script.Serialization;
using CoreAudioApi;
using System.IO;

namespace Wox.Plugin.Volume
{
    public class Main : IPlugin
    {
        private PluginInitContext context;
        private MMDevice PlaybackDevice;
        private Config config;
        private bool initialized = false;

        public void Init(PluginInitContext context)
        {
            this.context = context;

            try
            {
                string json = (new StreamReader(context.CurrentPluginMetadata.PluginDirectory + "\\config.json")).ReadToEnd();
                config = (new JavaScriptSerializer()).Deserialize<Config>(json);
            }
            catch (System.Exception)
            {
                return;
            }

            try
            {
                PlaybackDevice = MMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }
            catch (CoreAudioException)
            {
                return;
            }

            initialized = true;
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            results.Add(GetResult(query));
            return results;
        }

        private Result GetResult(Query query)
        {
            Result result = new Result()
            {
                Title = "Current volume",
                SubTitle = "no info",
                IcoPath = "Images\\icon.png"
            };

            if (!initialized)
            {
                return result;
            }

            string queryString = query.ToString();
            string lastChar = queryString.Substring(queryString.Length - 1);

            int volume = GetVolume();
            if (lastChar.Equals(config.up))
            {
                volume += config.step;
            }
            else if (lastChar.Equals(config.down))
            {
                volume -= config.step;
            }

            SetVolume(volume);
            volume = GetVolume();

            string volumeDesc = "[";
            for (int i = 0; i < 100; i += config.step)
            {
                volumeDesc = string.Concat(volumeDesc, volume > i ? "=" : "-");
            }
            volumeDesc = string.Concat(volumeDesc, "]");

            result.SubTitle = volumeDesc;
            return result;
        }

        public int GetVolume()
        {
            return (int) (PlaybackDevice.AudioEndpointVolumeEx.GetMasterVolumeLevelScalar() * 100);
        }

        public void SetVolume(int volume)
        {
            if (volume < 0)
            {
                volume = 0;
            }
            else if (volume > 100)
            {
                volume = 100;
            }

            PlaybackDevice.AudioEndpointVolumeEx.SetMasterVolumeLevelScalar(volume / 100.0f);
        }

    }
}
