using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;








namespace LaceupMigration
{
    public class OrderHistory
    {
        public int ClientId { get; set; }
        public int ProductId { get; set; }
        public DateTime When { get; set; }

        public float Old_Qty { get; set; }
        public UnitOfMeasure Old_UoM { get; set; }
        public double Old_Price { get; set; }
        public int Old_OfferType { get; set; }

        public float Dumps_Qty { get; set; }
        public UnitOfMeasure Dumps_UoM { get; set; }
        public double Dumps_Price { get; set; }
        public string Dumps_DetailUniqueId { get; set; }

        public float Returns_Qty { get; set; }
        public UnitOfMeasure Returns_UoM { get; set; }
        public double Returns_Price { get; set; }
        public string Returns_DetailUniqueId { get; set; }

        public float Count_Qty { get; set; }
        public UnitOfMeasure Count_UoM { get; set; }

        public bool WasCounted { get; set; }

        public float Sold_Qty { get; set; }
        public UnitOfMeasure Sold_UoM { get; set; }

        public float Invoice_Qty { get; set; }
        public UnitOfMeasure Invoice_UoM { get; set; }
        public double Invoice_Price { get; set; }
        public int Invoice_OfferType { get; set; }
        public string Invoice_DetailUniqueId { get; set; }

        public string ExtraFields { get; set; }

        public string ClientUniqueId { get; set; }

        static List<OrderHistory> history = new List<OrderHistory>();

        public static List<OrderHistory> History { get { return history; } }

        public float New_Qty
        {
            get
            {
                var counted = Count_Qty;
                if (Count_UoM != null)
                    counted *= Count_UoM.Conversion;

                var sold = Invoice_Qty;

                if (Invoice_UoM != null)
                    sold *= Invoice_UoM.Conversion;

                if (counted < 0)
                    counted = 0;
                return counted + sold;
            }
        }

        UnitOfMeasure newQtyUom = null;
        public UnitOfMeasure New_UoM
        {
            get
            {
                if (newQtyUom == null && Product != null && !string.IsNullOrEmpty(Product.UoMFamily))
                    newQtyUom = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == Product.UoMFamily && x.IsBase);
                return newQtyUom;
            }
        }

        public double New_Price { get { return Invoice_Price; } }

        public string OrderUniqueId { get; set; }
        public string RelationUniqueId { get; set; }

        Product prod = null;
        public Product Product
        {
            get
            {
                if (prod == null)
                    prod = Product.Find(ProductId);
                return prod;
            }
        }
    }
}