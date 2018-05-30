using System;
using System.IO;
using Microsoft.DirectX.AudioVideoPlayback;

namespace Sound.Players
{

    public class PlayerDirectX : ISoundPlayer
    {

        #region fields

        private string _trackPath = "";
        private Audio _trackToPlay = null;

        private object _locker = new object();

        #endregion




        #region prop

        public SoundPlayerType PlayerType { get; }

        #endregion




        #region ctor

        public PlayerDirectX()
        {
            PlayerType = SoundPlayerType.DirectX;
        }

        #endregion




        #region Methode

        public bool PlayFile(string file)
        {
            lock (_locker)
            {
                if (_trackToPlay != null)
                {
                    _trackToPlay.Dispose();
                    _trackToPlay = null;
                }

                _trackPath = file;
                if (_trackPath == "")
                    return false;

                try
                {
                    if (File.Exists(_trackPath) == true)
                    {
                        _trackToPlay = new Audio(_trackPath);
                        _trackToPlay.Play();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
                return false;
            }
        }



        public void Play()
        {
            lock (_locker)
            {
                if (_trackToPlay == null)
                    return;

                try
                {
                    _trackToPlay.Play();
                    return;
                }
                catch (Exception e)
                {
                    // ignored
                }
                return;
            }
        }



        public void Pause()
        {
            if (_trackToPlay == null)
                return;

            try
            {
                _trackToPlay.Pause();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public float GetVolume()
        {
            lock (_locker)
            {
                if (_trackToPlay != null)
                    return _trackToPlay.Volume;

                return 0;
            }
        }

        public void SetVolume(float volume)
        {
            lock (_locker)
            {
                if (_trackToPlay != null)
                {
                    _trackToPlay.Volume = (int)volume;
                }
            }
        }

        public long GetCurrentPosition()
        {
            try
            {
                if (_trackToPlay != null)
                    return (int)_trackToPlay.CurrentPosition;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            };

            return 0;
        }

        public TimeSpan? GetDuration()
        {
            lock (_locker)
            {
                try
                {
                    if (_trackToPlay != null)
                        return TimeSpan.FromSeconds(_trackToPlay.Duration);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return TimeSpan.FromSeconds(0);
                }

                return TimeSpan.FromSeconds(0);
            }
        }

        public SoundPlayerStatus GetPlayerStatus()
        {
            lock (_locker)
            {
                SoundPlayerStatus playerStatus = SoundPlayerStatus.Idle;

                try
                {
                    if (_trackToPlay != null)
                    {
                        if (_trackToPlay.Playing)
                        {
                            if (_trackToPlay.CurrentPosition >= _trackToPlay.Duration)
                                return SoundPlayerStatus.Idle;

                            return SoundPlayerStatus.Playing;
                        }

                        if (_trackToPlay.Paused)
                            return SoundPlayerStatus.Paused;

                        if (_trackToPlay.Paused)
                            return SoundPlayerStatus.Stop;

                        return SoundPlayerStatus.Error;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return playerStatus;
            }
        }

        #endregion




        #region Dispouse

        public void Dispose()
        {
            if (_trackToPlay != null && !_trackToPlay.Disposed)
            {
                _trackToPlay.Dispose();
            }
        }

        #endregion

    }
}
