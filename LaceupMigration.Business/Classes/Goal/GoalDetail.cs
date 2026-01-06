namespace LaceupMigration
{
    using System;
    using System.Linq;

    public partial class GoalDetail
    {
        public int Id { get; set; }
        public int GoalId { get; set; }
        public int ProductId { get; set; }
        public int SalesmanId { get; set; }
        public double Qty { get; set; }
        public double Amount { get; set; }
        public int UnitOfMeasureId { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedOn { get; set; }
        public byte[] RowVersion { get; set; }
        public Goal Goal { get; set; }
        public Product Product
        {
            get
            {
                return Product.Find(ProductId);
            }
        }
        public Salesman Salesman
        {
            get
            {
                return Salesman.List.FirstOrDefault(x => x.Id == SalesmanId);
            }
        }
        public UnitOfMeasure UoM
        {
            get
            {
                return UnitOfMeasure.List.FirstOrDefault(x => x.Id == UnitOfMeasureId);
            }
        }
    }
}
