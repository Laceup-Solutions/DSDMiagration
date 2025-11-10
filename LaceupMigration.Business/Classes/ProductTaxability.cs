using System.Collections.Generic;

namespace LaceupMigration
{
    class ProductTaxability
    {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public int ProductId { get; set; }

        public bool Taxed { get; set; }

        public string ExtraFields { get; set; }

        public double TaxRate{ get; set; }

        public static List<ProductTaxability> List { get; } = new List<ProductTaxability>();
    }
}