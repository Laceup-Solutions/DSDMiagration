using System.Collections.Generic;

namespace LaceupMigration
{

	/// <summary>
	/// Defines a price for either a Product/Client or Product/PriceLevel combination
	/// Use IsBasedOnPriceLevel to decide which combination to use.
	/// </summary>
	public class PriceLevel
	{
		public int Id { get; set; }

        public string Name { get; set; }

		public string ExtraFields { get; set; }

        static List<PriceLevel> priceList = new List<PriceLevel>();

		/// <summary>
		/// Returns the list of ProductPrice defined in the system
		/// </summary>
		public static List<PriceLevel> List
		{
			get { return priceList; }
		}

		/// <summary>
		/// Adds a new ProductPrice
		/// </summary>
		/// <param name="item"></param>
		public static void Add(PriceLevel item)
		{
			priceList.Add(item);
		}
	}
}

