using System;
using Library.Logs;
using NAudio.Wave;

namespace Sound.Players
{
    public class PlayerNAudio : ISoundPlayer
    {

       #region field

        private object _locker= new object();
        private readonly Log _loggerSoundPlayer = new Log("Sound.SoundQueue");

        #endregion




        #region prop

        private IWavePlayer WaveOutDevice { get; set; }
        private AudioFileReader AudioFileReader { get; set; }

        #endregion




        #region Methode

        public bool PlayFile(string file)
        {
            lock (_locker)
            {
                if (AudioFileReader != null)
                {
                    AudioFileReader.Dispose();
                    AudioFileReader = null;
                }

                try
                {
                    if (System.IO.File.Exists(file))
                    {
                        AudioFileReader = new AudioFileReader(file);

                        WaveOutDevice?.Stop();
                        WaveOutDevice?.Dispose();
                        WaveOutDevice = new WaveOut();

                        WaveOutDevice.Init(AudioFileReader);

                        SetVolume(0.9f);
                        Play();

                        return true;
                    }

                    _loggerSoundPlayer.Info($"PlayFile In player: {file} FILE NOT FOUND ????????????????????");
                }
                catch (Exception ex)
                {
                    _loggerSoundPlayer.Info($"PlayFile In player: ECXEPTION {ex.Message} !!!!!!!!!!!!!!!!!!!!");
                }

                return false;
            }
        }



        public void Play()
        {
            if (AudioFileReader == null)
            {
                _loggerSoundPlayer.Info($"PlayFile In Play methode: AudioFileReader == null !!!!!!!!!!!!!!!!!!!!");
                return;
            }

            try
            {
                if (WaveOutDevice.PlaybackState == PlaybackState.Paused ||
                    WaveOutDevice.PlaybackState == PlaybackState.Stopped)
                {
                    WaveOutDevice.Play();
                }
            }
            catch (Exception ex)
            {
                _loggerSoundPlayer.Info($"PlayFile In Play methode: ECXEPTION {ex.Message} !!!!!!!!!!!!!!!!!!!!");
                throw;
            }
        }



        public void Pause()
        {
            if (AudioFileReader == null)
                return;

            try
            {
                if (WaveOutDevice.PlaybackState == PlaybackState.Playing)
                    WaveOutDevice.Pause();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }



        public void Stop()
        {
            if (AudioFileReader == null)
                return;

            try
            {
                if (WaveOutDevice.PlaybackState == PlaybackState.Playing)
                    WaveOutDevice.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }



        public float GetVolume()
        {
            return AudioFileReader?.Volume ?? 0f;
        }



        //1.0f is full volume
        public void SetVolume(float volume)
        {
            if (AudioFileReader != null)
            {
                AudioFileReader.Volume = volume;
            }
        }



        public long GetCurrentPosition()
        {
            return  AudioFileReader?.Position ?? 0;
        }



        public TimeSpan? GetDuration()
        {
            return AudioFileReader?.TotalTime ?? null;
        }

        public SoundPlayerStatus GetPlayerStatus()
        {
            var state = WaveOutDevice?.PlaybackState;
            switch (state)
            {
                case null:
                case PlaybackState.Stopped:
                    return SoundPlayerStatus.Stop;

                case PlaybackState.Playing:
                    return SoundPlayerStatus.Playing;

                case PlaybackState.Paused:
                    return SoundPlayerStatus.Paused;

                default:
                    return SoundPlayerStatus.Idle;
            }
        }


        //public PlaybackState GetStatus()
        //{
        //    return WaveOutDevice?.PlaybackState ?? PlaybackState.Stopped;
        //}

        #endregion





        #region IDisposable

        public void Dispose()
        {
            if (WaveOutDevice != null)
            {
                WaveOutDevice.Stop();
                WaveOutDevice.Dispose();
            }
        }

        #endregion
    }
}
