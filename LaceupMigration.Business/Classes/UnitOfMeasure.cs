using System;
using System.Collections.Generic;

namespace LaceupMigration
{
	public class UnitOfMeasure
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public bool IsBase { get; set; }

		public string FamilyId { get; set; }

		public float Conversion { get; set; }

		public bool IsDefault { get; set; }

		public string OriginalId { get; set; }

        public string DefaultPurchase { get; set; }

        public bool IsActive { get; set; }

        public bool CreatedLocally { get; set; }

        public string FamilyName { get; set; }

        public string ExtraFields { get; set; }

        public static List<UnitOfMeasure> List = new List<UnitOfMeasure>();

        public static List<UnitOfMeasure> InactiveUoM = new List<UnitOfMeasure>();
	}
}

