using System;
using System.Collections.Generic;
using CoreAudioApi;

namespace Wox.Plugin.Volume
{
    public class Main : IPlugin
    {
        const int VOLUME_STEP = 5;
        private MMDevice PlaybackDevice;
        private bool initialized = false;

        public void Init(PluginInitContext context)
        {
            try
            {
                PlaybackDevice = MMDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                initialized = true;
            } catch (CoreAudioException e)
            {
            }
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            addResult(results, query);
            return results;
        }

        private void addResult(List<Result> results, Query query)
        {
            if (!initialized)
            {
                results.Add(new Result()
                {
                    Title = "Current volume",
                    SubTitle = "no info",
                    IcoPath = "Images\\icon.png"
                });
                return;
            }

            string queryString = query.ToString();
            string lastChar = queryString.Substring(queryString.Length - 1);

            int volume = GetVolume();
            if (lastChar.Equals("+"))
            {
                volume += VOLUME_STEP;
            }
            else if (lastChar.Equals("-"))
            {
                volume -= VOLUME_STEP;
            }

            SetVolume(volume);
            volume = GetVolume();

            string volumeDesc = "[";
            for (int i = 0; i < 100; i += VOLUME_STEP)
            {
                volumeDesc = string.Concat(volumeDesc, volume > i ? "=" : "-");
            }
            volumeDesc = string.Concat(volumeDesc, "]");

            results.Add(new Result()
            {
                Title = "Current volume",
                SubTitle = volumeDesc,
                IcoPath = "Images\\icon.png"
            });
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
