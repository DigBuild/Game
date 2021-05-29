using System;
using DigBuild.Platform.Audio;

namespace DigBuild.Audio
{
    public sealed class PlayingSound
    {
        private readonly Sound _sound;
        private SoundProperties _properties;

        internal AudioPlayer Player = null!;
        internal bool Ready;

        public bool IsPlaying { get; private set; } = true;

        public event Action? Ended;

        public float Gain
        {
            get => _properties.Gain;
            set => _properties.Gain = value;
        }

        public float Pitch
        {
            get => _properties.Pitch;
            set => _properties.Pitch = value;
        }

        public PlayingSound(Sound sound, SoundProperties properties)
        {
            _sound = sound;
            _properties = properties;
        }

        internal void Setup(AudioPlayer player)
        {
            Player = player;
            Ready = true;

            player.Gain = _properties.Gain;
            player.Pitch = _properties.Pitch;
            player.Play(_sound.Clip, _properties.Loop);
        }

        internal bool Tick()
        {
            Player.Gain = _properties.Gain;
            Player.Pitch = _properties.Pitch;
            if (Player.Status != AudioPlayer.PlayStatus.Stopped)
                return true;

            IsPlaying = false;
            Ended?.Invoke();
            return false;
        }
    }
}