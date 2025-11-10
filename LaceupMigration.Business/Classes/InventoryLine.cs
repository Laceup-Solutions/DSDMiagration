using System;

namespace LaceupMigration
{
	public class InventoryLine 
	{

		public Product Product{ get; set; }

		public float Real{ get; set; }

		public float Starting{ get; set; }

        public UnitOfMeasure UoM { get; set; }

        public string UniqueId { get; set; }

        public float Weight { get; set; }

        public bool CurrentFocus { get; set; }

        public string Lot { get; set; }

        public DateTime Expiration { get; set; }
        
        public double QtyCases { get; set; }

        public InventoryLine()
        {
            Lot = "";
        }
    }
}

