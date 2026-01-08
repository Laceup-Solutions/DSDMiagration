





using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LaceupMigration
{
    public class OrdersInOS 
    {
        public static List<OrdersInOS> List = new List<OrdersInOS>();

        public int PrintedCopies { get; set; }
        public int OrderId { get; set; }

        public int OriginalOrderId { get; set; }

        public int SalesmanId { get; set; }

        public DateTime Date { get; set; }

        public OrderType OrderType;

        public OrderStatus OrderStatus;

        public string Comments { get; set; }

        public string PrintedOrderId { get; set; }

        public Client Client { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public float TaxRate { get; set; }

        public DiscountType DiscountType { get; set; }

        public float DiscountAmount { get; set; }

        public string DiscountComment { get; set; }

        public bool Voided { get; set; }

        public string PONumber { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime ShipDate { get; set; }

        public int BatchId { get; set; }

        public bool Dexed { get; set; }

        public bool Finished { get; set; }

        public bool Reshipped { get; set; }


        public string CompanyName { get; set; }

        public string ExtraFields { get; set; }
        public string UniqueId { get; set; }
        public int OriginalSalesmanId { get; set; }
        
        public double OrderTotalCost
        {
            get
            {
                var isCredit = OrderType == OrderType.Credit || OrderType == OrderType.Return;

                var factor = isCredit ? -1 : 1;
                    return (Details.Sum(x => (x.Qty * x.Price)) * factor);
            }
        }

        public bool AsPresale
        {
            get
            {
                return ((int)OrderStatus < 6);
            }
        }
        public List<StatusOrderDetail> Details = new List<StatusOrderDetail>();
    }

    public class StatusOrderDetail
    {
        public int OrderDetailId { get; set; }
        public double Price { get; set; }

        public double ExpectedPrice { get; set; }

        public string Comments { get; set; }
        public Product Product { get; set; }
        public OrdersInOS Order { get; set; }

        public float Qty { get; set; }

        public string Lot { get; set; }

        public bool Damaged { get; set; }

        public float Ordered { get; set; }

        public string OriginalId { get; set; }

        public bool IsCredit { get; set; }

        public UnitOfMeasure UnitOfMeasure { get; set; }
        public bool Deleted { get; set; }

        public float Weight { get; set; }

        public double DexPrice { get; set; }

        public int RelatedOrderDetail { get; set; }
        public string ExtraFields { get; set; }
        public bool Taxed { get; set; }

        public double TaxRate { get; set; }

        public double Discount { get; set; }

        public DiscountType DiscountType { get; set; }

        public UnitOfMeasure OriginalUoM { get; set; }

        public int ReasonId { get; set; }
        public bool FromOffer { get; set; }

    }
}