using System;
using LaceupMigration;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// Result from the template Edit Line popup (single popup for Price, Weight, Qty, Comments, FreeItem, etc.).
    /// </summary>
    public class TemplateEditLineResult
    {
        public float Qty { get; set; }
        public double Weight { get; set; }
        public double Price { get; set; }
        public string Comments { get; set; } = "";
        public bool FreeItem { get; set; }
        public string Lot { get; set; } = "";
        public DateTime LotExpiration { get; set; }
        /// <summary>Selected UoM when product has UnitOfMeasures; otherwise null (use existing).</summary>
        public UnitOfMeasure? SelectedUoM { get; set; }
    }
}
