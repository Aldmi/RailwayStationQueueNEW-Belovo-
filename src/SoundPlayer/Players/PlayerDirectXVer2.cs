using System;
using Microsoft.DirectX.AudioVideoPlayback;

namespace Sound.Players
{
    public class PlayerDirectXVer2 : ISoundPlayer
    {

        #region fields

        private string _trackPath = "";
        public Audio _trackToPlay = null;

        private object _locker = new object();

        #endregion



        public PlayerDirectXVer2()
        {
           // Audio _trackToPlay = null;
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool PlayFile(string file)
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public float GetVolume()
        {
            throw new NotImplementedException();
        }

        public void SetVolume(float volume)
        {
            throw new NotImplementedException();
        }

        public long GetCurrentPosition()
        {
            throw new NotImplementedException();
        }

        public TimeSpan? GetDuration()
        {
            throw new NotImplementedException();
        }

        public SoundPlayerStatus GetPlayerStatus()
        {
            throw new NotImplementedException();
        }
    }
}