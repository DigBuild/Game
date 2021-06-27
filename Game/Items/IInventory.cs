namespace DigBuild.Items
{
    public interface IInventory
    {
        IInventoryTransaction BeginTransaction();
    }
}