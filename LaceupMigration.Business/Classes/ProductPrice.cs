using System.Collections.Generic;

namespace LaceupMigration
{

	/// <summary>
	/// Defines a price for either a Product/Client or Product/PriceLevel combination
	/// Use IsBasedOnPriceLevel to decide which combination to use.
	/// </summary>
	public class ProductPrice
	{

		/// <summary>
		/// Returns the product associated with this price
		/// </summary>
		public int ProductId { get; set; }

		/// <summary>
		/// Returns the Client associated with ths price or null if this price is based on PriceLevel
		/// </summary>
		public int ClientId { get; set; }

		/// <summary>
		/// Returns the PriceLevelID. Valid only if IsBasedOnPriceLevel is true
		/// </summary>
		public int PriceLevelId { get; set; }

		/// <summary>
		/// Indicates if the pricing is based on the Client or in a PriceLevel
		/// </summary>
		public bool IsBasedOnPriceLevel { get; set; }

		/// <summary>
		/// The Price
		/// </summary>
		public double Price { get; set; }

        public double Allowance { get; set; }

        public string Extrafields { get; set; }

        public string PartNumber { get; set; }

        static List<ProductPrice> priceList = new List<ProductPrice> ();

		/// <summary>
		/// Returns the list of ProductPrice defined in the system
		/// </summary>
		public static IEnumerable<ProductPrice> Pricelist {
			get { return priceList; }
		}

		/// <summary>
		/// Clear the list.
		/// </summary>
		/// <param name="count"></param>
		public static void Clear (int count)
		{
			priceList.Clear ();
			priceList.Capacity = count;
		}

		/// <summary>
		/// Adds a new ProductPrice
		/// </summary>
		/// <param name="item"></param>
		public static void Add (ProductPrice item)
		{
			priceList.Add (item);
		}
	}
}

