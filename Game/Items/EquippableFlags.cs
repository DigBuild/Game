using System;

namespace DigBuild.Items
{
    [Flags]
    public enum EquippableFlags : byte
    {
        NotEquippable = 0,
        Helmet = 1 << 0,
        Chestplate = 1 << 1,
        Leggings = 1 << 2,
        Boots = 1 << 3,
        Equipment = 1 << 4
    }
}