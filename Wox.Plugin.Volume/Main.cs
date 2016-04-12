using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.IO;

namespace Wox.Plugin.Volume
{
    public class Main : IPlugin
    {
        private PluginInitContext context;
        private Manager volumeManager;
        private Config config;
        private bool initialized = false;
        private string initializaionError;
        private string lastQuery = "";

        public void Init(PluginInitContext context)
        {
            this.context = context;
            
            // 1. read the config.json
            try
            {
                using (StreamReader sr = new StreamReader(context.CurrentPluginMetadata.PluginDirectory + "\\config.json"))
                {
                    config = (new JavaScriptSerializer()).Deserialize<Config>(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                initializaionError = e.Message;
                return;
            }

            // 2. initialize the volume handling
            try
            {
                volumeManager = new Manager();
            }
            catch (Exception e)
            {
                initializaionError = e.Message;
                return;
            }

            // 3. all good
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
                SubTitle = initializaionError,
                IcoPath = "Images\\icon.png"
            };

            // initialization failed, we can't process the input
            if (!initialized)
            {
                return result;
            }

            string queryString = query.RawQuery;

            // do nothing if the config says so and the user deleted the input
            if (config.applyOnDelete || !config.applyOnDelete && lastQuery.Length < queryString.Length)
            {
                // process the last char
                ApplyKey(
                    queryString.Substring(queryString.Length - 1)
                );
            }
            
            // write the volume description in the result
            result.SubTitle = GetVolumeDescription();

            // store the query as last query
            lastQuery = queryString;

            // change the icon if muted
            if (volumeManager.IsMute())
            {
                result.IcoPath = "Images\\mute.png";
            }

            return result;
        }

        private string GetVolumeDescription()
        {
            if (volumeManager.IsMute())
            {
                return "[X]";
            }

            int volume = volumeManager.GetVolume();
            string volumeDesc;
            switch (config.style)
            {
                case Config.STYLE_PERCENT:
                    volumeDesc = volume + "%";
                    break;

                case Config.STYLE_BAR:
                    volumeDesc = "[";
                    for (int i = 0; i < 100; i += config.step)
                    {
                        volumeDesc = string.Concat(volumeDesc, volume > i ? "=" : "-");
                    }
                    volumeDesc = string.Concat(volumeDesc, "]");
                    break;

                default:
                    volumeDesc = "";
                    break;
            }

            return volumeDesc;
        }

        private void ApplyKey(string lastChar)
        {
            if (lastChar.Equals(config.mute))
            {
                volumeManager.ToggleMute();
            }

            if (volumeManager.IsMute())
            {
                return;
            }

            int volume = volumeManager.GetVolume();
            if (lastChar.Equals(config.up))
            {
                volume += config.step;
            }
            else if (lastChar.Equals(config.down))
            {
                volume -= config.step;
            }

            volumeManager.SetVolume(volume);
        }

    }
}
