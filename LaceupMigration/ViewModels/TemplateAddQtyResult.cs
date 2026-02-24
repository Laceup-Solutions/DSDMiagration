using System;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// Result from the template Add Qty popup (single popup for all inputs).
    /// </summary>
    public class TemplateAddQtyResult
    {
        /// <summary>Number of lines to insert.</summary>
        public int LineCount { get; set; } = 1;
        public float Qty { get; set; }
        public double Price { get; set; }
        public double Weight { get; set; }
        public string Lot { get; set; } = "";
        public DateTime LotExpiration { get; set; }
    }
}
