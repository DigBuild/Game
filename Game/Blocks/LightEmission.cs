using DigBuild.Engine.Math;

namespace DigBuild.Blocks
{
    /// <summary>
    /// A block's light emission pattern.
    /// </summary>
    public readonly struct LightEmission
    {
        /// <summary>
        /// The local light value.
        /// </summary>
        public readonly byte Local;
        /// <summary>
        /// The emission on the negative X direction.
        /// </summary>
        public readonly byte NegX;
        /// <summary>
        /// The emission on the positive X direction.
        /// </summary>
        public readonly byte PosX;
        /// <summary>
        /// The emission on the negative Y direction.
        /// </summary>
        public readonly byte NegY;
        /// <summary>
        /// The emission on the positive Y direction.
        /// </summary>
        public readonly byte PosY;
        /// <summary>
        /// The emission on the negative Z direction.
        /// </summary>
        public readonly byte NegZ;
        /// <summary>
        /// The emission on the positive Z direction.
        /// </summary>
        public readonly byte PosZ;

        public LightEmission(byte local, byte negX, byte posX, byte negY, byte posY, byte negZ, byte posZ)
        {
            Local = (byte) (local & 0xF);
            NegX = (byte) (negX & 0xF);
            PosX = (byte) (posX & 0xF);
            NegY = (byte) (negY & 0xF);
            PosY = (byte) (posY & 0xF);
            NegZ = (byte) (negZ & 0xF);
            PosZ = (byte) (posZ & 0xF);
        }

        public LightEmission(byte emission) :
            this(emission, emission, emission, emission, emission, emission, emission)
        {
        }

        /// <summary>
        /// Gets the emission in a given direction.
        /// </summary>
        /// <param name="direction">The direction</param>
        /// <returns>The emission</returns>
        public byte Get(Direction direction)
        {
            return direction switch
            {
                Direction.NegX => NegX,
                Direction.PosX => PosX,
                Direction.NegY => NegY,
                Direction.PosY => PosY,
                Direction.NegZ => NegZ,
                Direction.PosZ => PosZ,
                _ => Local
            };
        }
    }
}