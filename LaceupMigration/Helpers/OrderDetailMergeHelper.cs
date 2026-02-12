using System;
using System.Linq;

namespace LaceupMigration.Helpers
{
    /// <summary>
    /// After modifying an order detail (e.g. changing UoM from Each to Case so price matches another line),
    /// check if there is another detail with same Product, UoM, Price, and item type (IsCredit, Damaged).
    /// If found, merge the two into one (add qtys, remove the duplicate).
    /// </summary>
    public static class OrderDetailMergeHelper
    {
        /// <summary>
        /// If another detail exists with same ProductId, UoM, Price, IsCredit, and Damaged,
        /// add its qty (and weight if applicable) to <paramref name="detailToKeep"/>, delete the other, and return true.
        /// Otherwise return false.
        /// </summary>
        public static bool TryMergeDuplicateDetail(Order order, OrderDetail detailToKeep)
        {
            if (order?.Details == null || detailToKeep == null || detailToKeep.Deleted)
                return false;

            var productId = detailToKeep.Product?.ProductId ?? 0;
            if (productId == 0)
                return false;

            var other = order.Details.FirstOrDefault(d =>
                !d.Deleted
                && d != detailToKeep
                && d.Product?.ProductId == productId
                && SameUoM(d.UnitOfMeasure, detailToKeep.UnitOfMeasure)
                && SamePrice(d.Price, detailToKeep.Price)
                && d.IsCredit == detailToKeep.IsCredit
                && d.Damaged == detailToKeep.Damaged);

            if (other == null)
                return false;

            // Merge: add the other's qty (and weight) into the detail we're keeping, then delete the other
            detailToKeep.Qty += other.Qty;
            if (detailToKeep.Product?.SoldByWeight == true)
                detailToKeep.Weight += other.Weight;

            order.DeleteDetail(other);
            return true;
        }

        private static bool SameUoM(UnitOfMeasure a, UnitOfMeasure b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Id == b.Id;
        }

        private static bool SamePrice(double a, double b)
        {
            var tolerance = Math.Pow(10, -Config.Round);
            return Math.Abs(a - b) < tolerance;
        }
    }
}
