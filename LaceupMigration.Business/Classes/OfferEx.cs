using System;
using System.Collections.Generic;

namespace LaceupMigration
{
    class OfferEx
    {
        static List<OfferEx> categoryList = new List<OfferEx>();

        public static void Add(OfferEx category)
        {
            categoryList.Add(category);
        }

        /// <summary>
        /// Returns the list of the categories.
        /// </summary>
        public static List<OfferEx> List
        {
            get { return categoryList; }
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public int OfferType { get; set; }
        public Nullable<double> Price { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<double> TriggerQty { get; set; }
        public Nullable<double> DiscountedQty { get; set; }
        public Nullable<double> DiscountedPrice { get; set; }
        public Nullable<int> TriggerUnitOfMeasureId { get; set; }
        public Nullable<int> DiscountedUnitOfMeasureId { get; set; }
        public Nullable<int> DiscountedProductId { get; set; }
        public string BreaksAsString { get; set; }
        public Nullable<int> OriginGroup { get; set; }
        public Nullable<int> OriginProductCategory { get; set; }
        public bool Primary { get; set; }
        public bool Recurrent { get; set; }
        public string ExtraFields { get; set; }
        public Nullable<int> DateUsed { get; set; }
    }
}