using System;

namespace LaceupMigration
{
	public class LoadOrderDetail
	{
		public Product Product { get; set; }
		public float Qty { get; set; }
        public UnitOfMeasure UoM { get; set; }
		public string Comments { get; set; }
		
		public string Lot { get; set; }
	}
}

