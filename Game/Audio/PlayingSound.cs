using System;
using DigBuild.Platform.Audio;

namespace DigBuild.Audio
{
    /// <summary>
    /// A playing sound.
    /// </summary>
    public sealed class PlayingSound
    {
        private readonly Sound _sound;
        private PlaybackProperties _properties;

        internal AudioPlayer Player = null!;
        internal bool Ready;

        /// <summary>
        /// Whether the sound is actively playing.
        /// </summary>
        public bool IsPlaying { get; private set; } = true;

        /// <summary>
        /// Fired when the sound finishes playing.
        /// </summary>
        public event Action? Ended;

        /// <summary>
        /// The gain.
        /// </summary>
        public float Gain
        {
            get => _properties.Gain;
            set => _properties.Gain = value;
        }

        /// <summary>
        /// The pitch.
        /// </summary>
        public float Pitch
        {
            get => _properties.Pitch;
            set => _properties.Pitch = value;
        }

        public PlayingSound(Sound sound, PlaybackProperties properties)
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