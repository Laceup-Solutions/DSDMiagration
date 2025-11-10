namespace LaceupMigration
{
    public interface IInvoiceIdProvider
    {
        string GetId(Batch batch);

        string GetId(Order order);
    }
}