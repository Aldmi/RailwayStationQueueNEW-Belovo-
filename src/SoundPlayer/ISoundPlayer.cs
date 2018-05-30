using System;
using NAudio.Wave;

namespace Sound
{
    public enum SoundPlayerType
    {
        None, DirectX, Omneo
    }

    public enum SoundPlayerStatus
    {
        Idle,
        Error,
        Stop,
        Playing,
        Paused,
    };

    public interface ISoundPlayer : IDisposable
    {
        bool PlayFile(string file);
        void Play();
        void Pause();
        void Stop();

        float GetVolume();
        void SetVolume(float volume);

        long GetCurrentPosition();
        TimeSpan? GetDuration();

        SoundPlayerStatus GetPlayerStatus();
    }
}