namespace LaceupMigration
{
    using System;
    using System.Collections.Generic;

    public partial class ClientCategoryEx
    {
        public static List<ClientCategoryEx> List = new List<ClientCategoryEx>();
        public ClientCategoryEx()
        {
            this.Products = new HashSet<Product>();
            this.Clients = new HashSet<Client>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<Client> Clients { get; set; }
    }
}
