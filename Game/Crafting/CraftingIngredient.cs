using DigBuild.Engine.Items;

namespace DigBuild.Crafting
{
    /// <summary>
    /// A basic crafting ingredient implementation.
    /// </summary>
    public sealed class CraftingIngredient : ICraftingIngredient
    {
        /// <summary>
        /// An empty ingredient.
        /// </summary>
        public static readonly ICraftingIngredient None = new NoIngredient();

        private readonly Item _item;

        public CraftingIngredient(Item item)
        {
            _item = item;
        }

        public bool Test(ItemInstance item)
        {
            return item.Count > 0 && item.Type == _item;
        }

        public ItemInstance ConsumeOne(ItemInstance item)
        {
            return new ItemInstance(item.Type, (ushort) (item.Count - 1u));
        }

        private sealed class NoIngredient : ICraftingIngredient
        {
            public bool Test(ItemInstance item)
            {
                return item.Count == 0;
            }

            public ItemInstance ConsumeOne(ItemInstance item)
            {
                return ItemInstance.Empty;
            }
        }
    }
}