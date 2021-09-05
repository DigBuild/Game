using System;

namespace DigBuild.Items
{
    /// <summary>
    /// Flags that determine an item's equippability.
    /// </summary>
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