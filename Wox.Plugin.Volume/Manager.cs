using CoreAudio;

namespace Wox.Plugin.Volume
{
    class Manager
    {
        private MMDevice playbackDevice;

        public Manager()
        {
            playbackDevice = (new MMDeviceEnumerator()).GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
        }

        public int GetVolume()
        {
            return (int)(playbackDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
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

            playbackDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volume / 100.0f;
        }

        public void ToggleMute()
        {
            playbackDevice.AudioEndpointVolume.Mute = !IsMute();
        }

        public bool IsMute()
        {
            return playbackDevice.AudioEndpointVolume.Mute;
        }
    }
}
