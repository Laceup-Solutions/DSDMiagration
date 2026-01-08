namespace LaceupMigration
{

    public partial class ProductVisibleSalesman
    {
        public int Id { get; set; }
        public bool Visible { get; set; }
        public int ProductId { get; set; }
        public int SalesmanId { get; set; }

        public virtual Product Product { get; set; }
    }
}