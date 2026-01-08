#region Nested Types

using System.Collections.Generic;
using System;
using System.Linq;

namespace LaceupMigration
{
    public enum GoalCriteria
    {
        Product = 1,
        Route = 2,
        Payment = 3,
        Customer = 4,
        ProductsByCustomer = 5
    }

    public enum GoalType
    {
        Amount = 1,
        Qty = 2
    }

    public class OrderDTO 
    {
        #region Properties and Indexers

        public double? CalculatedTotal { get; set; }
        public DateTime DateReceived { get; set; }
        public List<OrderDetailDTO> Details { get; set; }
        public bool Finished { get; set; }
        public int Id { get; set; }
        public int SalesmanId { get; set; }

        #endregion
    }

    public class OrderDetailDTO 
    {
        #region Properties and Indexers

        public int Id { get; set; }
        public bool? IsCredit { get; set; }
        public double Price { get; set; }
        public int ProductId { get; set; }
        public double Qty { get; set; }

        #endregion
    }

    public class GoalProgressDTO 
    {
        public enum GoalStatus
        {
            Expired,
            Progressing,
            NoProgress
        }

        public GoalStatus Status
        {
            get
            {
                DateTime today = DateTime.Today;
                return EndDate < today ? GoalStatus.Expired :
                    StartDate > today ? GoalStatus.NoProgress : GoalStatus.Progressing;
            }
        }

        public static List<GoalProgressDTO> List = new List<GoalProgressDTO>();

        #region Properties

        public GoalCriteria Criteria { get; set; }
        public DateTime EndDate { get; set; }
        public int Id { get; set; }
        public virtual string Name { get; set; }
        public double QuantityOrAmount { get; set; }
        public double Sold { get; set; }
        public double SalesOrder { get; set; }
        public double CreditInvoice { get; set; }
        public DateTime StartDate { get; set; }
        public GoalType Type { get; set; }
        public int WorkedDays { get; set; }
        public int WorkingDays { get; set; }

        public List<GoalDetailDTO> Details = new List<GoalDetailDTO>();
        public int PendingDays => WorkingDays - WorkedDays;
        public double PendingDaysPercent => Math.Round((double)PendingDays / WorkingDays, 4);
        public double WorkedDaysPercent => Math.Round((double)WorkedDays / WorkingDays, 4);

        public bool IsActive { get; set; }
        #endregion
    }

    public class GoalDetailDTO 
    {
        public static List<GoalDetailDTO> List = new List<GoalDetailDTO>();

        public GoalProgressDTO Goal { get { return GoalProgressDTO.List.FirstOrDefault(x => x.Id == GoalId); } }

        #region Properties
        public Invoice ExternalInvoice { get; set; }
        public int ClientId { get; set; }
        public int GoalId { get; set; }
        public Client client { get { return Client.Find(ClientId); } }
        public string ClientName { get; set; }
        public int Id { get; set; }
        public virtual string Name { get; set; }
        public int? ProductId { get; set; }
        public double? QuantityOrAmount { get; set; }
        public int? SalesmanId { get; set; }
        public double? SalesOrder
        {
            get; set;
        }

        public double CreditInvoice { get; set; }

        public double? Sold
        {
            get; set;
        }

        public Product Product
        {
            get
            {
                return Product.Find(ProductId ?? 0);
            }
        }

        public UnitOfMeasure UoM { get; set; }
        public UnitOfMeasure ChangedUoM { get; set; }
        public GoalType Type { get; set; }
        public GoalCriteria Criteria { get; set; }
        public int WorkedDays { get; set; }
        public int WorkingDays { get; set; }
        public List<OrderDTO> Orders { get; set; }
        public double Accomplished => AccomplishedPercent * 100;
        public double AccomplishedPercent =>
            Sold.HasValue && QuantityOrAmount.HasValue && QuantityOrAmountValue > 0
                ? Math.Round(SoldValue / QuantityOrAmountValue, 4)
                : 0;
        public double AllSalesPercent => AccomplishedPercent + SalesOrderPercent;
        public double DailySalesPerMonth => WorkingDays > 0 ? Math.Round(QuantityOrAmountValue / WorkingDays, 2) : 0;
        public double DailySalesToGoal =>
            WorkingDays - WorkedDays > 0
                ? Math.Round(
                    (QuantityOrAmountValue - SoldValue <= 0 ? 0 : QuantityOrAmountValue - SoldValue) /
                    (WorkingDays - WorkedDays), 2)
                : 0;

        public double DailySalesToGoalIncludeSales(bool includeCreditInvoices)
        {
            return WorkingDays - WorkedDays > 0
                ? Math.Round(
                    (QuantityOrAmountValue - (SoldValue + SalesOrderValue + (includeCreditInvoices ? CreditInvoice : 0)) <= 0 ? 0 : QuantityOrAmountValue - (SoldValue + SalesOrderValue + (includeCreditInvoices ? CreditInvoice : 0))) /
                    (WorkingDays - WorkedDays), 2)
                : 0;
        }

        public double DailySalesToGoalIncludeSalesInDevice(double sold)
        {
            return WorkingDays - WorkedDays > 0
                ? Math.Round(
                    (QuantityOrAmountValue - sold <= 0 ? 0 : QuantityOrAmountValue - sold) /
                    (WorkingDays - WorkedDays), 2)
                : 0;
        }

        public double DifferenceUpToday => Math.Round(SoldValue - GoalUpToday, 2);
        public double QuantityOrAmountValue => QuantityOrAmount ?? 0;
        public double GoalUpToday => Math.Round(QuantityOrAmountValue / WorkingDays * WorkedDays, 2);
        public double SalesOrderPercent =>
            SalesOrder.HasValue && QuantityOrAmount.HasValue && QuantityOrAmountValue > 0
                ? Math.Round(SalesOrderValue / QuantityOrAmountValue, 4)
                : 0;
        public double SalesOrderValue => SalesOrder ?? 0;
        public double SalesUpToday => SalesUpTodayPercent * 100;
        public double SalesUpTodayPercent => Math.Round(GoalUpToday > 0 ? SoldValue / GoalUpToday : 0, 4);
        public double SoldValue => Sold ?? 0;
        public double StartingAmountCollectedAlready { get; set; }

        #endregion
    }

    #endregion
}