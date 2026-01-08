using System.Linq;

namespace LaceupMigration
{
	public class SentOrderDetail
	{
		public int ProductId {
			set;
			get;
		}

		public Product GetProduct
		{
			get
			{
				var p = Product.Products.FirstOrDefault(x => x.ProductId == this.ProductId);

				if (p == null)
					p = Product.CreateNotFoundProduct(ProductId, true);

				return p;
			}
		}

		public float Qty {
			get;
			set;
		}

		public double Price {
			get;
			set;
		}

		public string Comments { get; set; }

		public UnitOfMeasure UoM { get; set; }

		public float Discount { get; set; }

		public DiscountType DiscountType {get;set;}
		public double ExpectedPrice { get; set; }

		public bool IsCredit { get; set; }

		public bool Damaged { get; set; }

		public float TaxRate { get; set; }

		public float Weight { get; set; }
        public bool Taxed { get; set; }

		public string ExtraFields { get; set; }
    }
}

