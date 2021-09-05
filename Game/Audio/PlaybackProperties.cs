namespace DigBuild.Audio
{
    /// <summary>
    /// A set of audio playback properties.
    /// </summary>
    public struct PlaybackProperties
    {
        /// <summary>
        /// The default playback properties.
        /// </summary>
        public static PlaybackProperties Default { get; } = new()
        {
            Loop = false,
            Gain = 1,
            Pitch = 1
        };

        /// <summary>
        /// Whether to loop at the end of playback.
        /// </summary>
        public bool Loop { get; set; }
        /// <summary>
        /// The gain.
        /// </summary>
        public float Gain { get; set; }
        /// <summary>
        /// The pitch.
        /// </summary>
        public float Pitch { get; set; }
    }
}