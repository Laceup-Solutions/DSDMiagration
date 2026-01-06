using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration
{

	/// <summary>
	/// Keeps the list of products authorized under a given client category
	/// </summary>
	public class ClientCategoryProducts
	{
		Dictionary<int, List<Product>> categoryProductsMap = new Dictionary<int, List<Product>> ();

		/// <summary>
		/// The client category
		/// </summary>
		public int CategoryId { get; set; }

		public IEnumerable<Product> Products {
			get;
			set;
		}

		public List<Product> ProductsInCategory (int categoryId)
		{
			if (categoryProductsMap.ContainsKey (categoryId))
				return categoryProductsMap [categoryId];


			List<Product> items = new List<Product> ();
			foreach (var p in Products)
				if (p.CategoryId == categoryId)
					items.Add (p);

			categoryProductsMap.Add (categoryId, items);
			
			return items;
		}

		/// <summary>
		/// The constructor of the type. It receives the clientCategoryId and the list of products under it
		/// </summary>
		/// <param name="id">The clientCategoryId</param>
		/// <param name="list">The list of products</param>
		public ClientCategoryProducts (int id, IEnumerable<Product> list)
		{
			this.CategoryId = id;
			this.Products = list;
		}

		private static List<ClientCategoryProducts> list = new List<ClientCategoryProducts> ();

		/// <summary>
		/// Add a new Category to the list
		/// </summary>
		/// <param name="item">Item to add</param>
		public static void AddToList (ClientCategoryProducts item)
		{
			list.Add (item);
		}

		/// <summary>
		/// Clear the list
		/// </summary>
		public static void ResetList ()
		{
			list.Clear ();
		}

		/// <summary>
		/// Giving a client categoryID return the ClientCategoryProducts associated with it
		/// </summary>
		/// <param name="categoryId">The ClientCategoryId</param>
		/// <returns>the ClientCategory or null if not found</returns>
		public static ClientCategoryProducts Find (int clientCategoryId)
		{
			var l = list.FirstOrDefault (x => x.CategoryId == clientCategoryId);
			return l ?? new ClientCategoryProducts (-1, new List<Product> ());
		}

        public static void RemoveNonVisible(List<ProductVisibleSalesman> visible)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var cat = list[i];

                var newProdList = new List<Product>();

                foreach (var prod in cat.Products)
                {
                    if (visible.FirstOrDefault(x => x.ProductId == prod.ProductId) != null)
                        newProdList.Add(prod);
                }

                if (newProdList.Count == 0)
                {
                    list.RemoveAt(i);
                    i--;
                }
                else
                    cat.Products = newProdList;
            }
        }
    }
}
