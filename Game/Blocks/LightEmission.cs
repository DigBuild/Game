using DigBuild.Engine.Math;

namespace DigBuild.Blocks
{
    public readonly struct LightEmission
    {
        public readonly byte Local;
        public readonly byte NegX, PosX;
        public readonly byte NegY, PosY;
        public readonly byte NegZ, PosZ;

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
                _ => 0
            };
        }
    }
}