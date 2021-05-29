namespace DigBuild.Audio
{
    public struct SoundProperties
    {
        public static SoundProperties Default { get; } = new()
        {
            Loop = false,
            Gain = 1,
            Pitch = 1
        };

        public bool Loop { get; set; }
        public float Gain { get; set; }
        public float Pitch { get; set; }
    }
}