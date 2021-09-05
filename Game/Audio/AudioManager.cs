using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using DigBuild.Platform.Audio;
using DigBuild.Platform.Resource;
using DigBuild.Registries;

namespace DigBuild.Audio
{
    /// <summary>
    /// An audio manager system.
    /// </summary>
    public class AudioManager
    {
        private readonly ResourceManager _resourceManager;

        private AudioSystem _audioSystem = null!;
        private bool _shouldStop;

        private readonly Queue<AudioPlayer> _availablePlayers = new();
        private readonly HashSet<PlayingSound> _playingSounds = new();

        private readonly ConcurrentQueue<PlayingSound> _queuedSounds = new();

        public AudioManager(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;

            new Thread(Run)
            {
                Name = "Audio Subsystem"
            }.Start();
        }

        private void Run()
        {
            _audioSystem = Platform.Platform.AudioSystem;

            foreach (var sound in GameRegistries.Sounds.Values)
            {
                sound.Load(_resourceManager, _audioSystem);
            }

            while (!_shouldStop)
            {
                while (_queuedSounds.TryDequeue(out var sound))
                {
                    if (!_availablePlayers.TryDequeue(out var player)) player = _audioSystem.CreatePlayer();

                    sound.Setup(player);
                    _playingSounds.Add(sound);
                }

                _playingSounds.RemoveWhere(sound =>
                {
                    var playing = sound.Tick();
                    if (playing) return false;
                    _availablePlayers.Enqueue(sound.Player);
                    return true;
                });

                Thread.Sleep(1000 / 20);
            }

            _audioSystem.Dispose();
        }

        /// <summary>
        /// Tells the audio system to shutdown as soon as possible.
        /// </summary>
        public void Stop()
        {
            _shouldStop = true;
        }
        
        /// <summary>
        /// Starts playing a sound.
        /// </summary>
        /// <param name="sound">The sound</param>
        /// <param name="loop">Whether to loop or not</param>
        /// <param name="gain">The gain</param>
        /// <param name="pitch">The pitch</param>
        /// <returns>The playing sound</returns>
        public PlayingSound Play(Sound sound, bool loop = false, float gain = 1, float pitch = 1)
        {
            return Play(sound, new PlaybackProperties()
            {
                Loop = loop,
                Gain = gain,
                Pitch = pitch
            });
        }
        
        /// <summary>
        /// Starts playing a sound.
        /// </summary>
        /// <param name="sound">The sound</param>
        /// <param name="properties">The playback properties</param>
        /// <returns>The playing sound</returns>
        public PlayingSound Play(Sound sound, PlaybackProperties properties)
        {
            var playingSound = new PlayingSound(sound, properties);
            _queuedSounds.Enqueue(playingSound);
            return playingSound;
        }
    }
}