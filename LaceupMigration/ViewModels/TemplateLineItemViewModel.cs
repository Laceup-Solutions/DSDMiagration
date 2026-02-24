using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;
using LaceupMigration;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// View model for a single template line (StandartTemplateLine or GroupedTemplateLine) in the order/credit template list.
    /// Matches Xamarin NewOrderTemplateActivity list cell: product ID + size + name, OH, Total Lb/Amount, Last Visit, Per week, Add button.
    /// </summary>
    public partial class TemplateLineItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _productName = string.Empty;

        /// <summary>Display: product ID + size/weight + name (e.g. "002 8-5lb Cotija Rallado FW"). Bold blue in cell.</summary>
        [ObservableProperty]
        private string _productDisplayName = string.Empty;

        [ObservableProperty]
        private string _amountText = string.Empty;

        [ObservableProperty]
        private string _totalQtyDisplay = "+";

        [ObservableProperty]
        private string _onHandText = string.Empty;

        /// <summary>"Total Lb: {value}" when SoldByWeight, else empty.</summary>
        [ObservableProperty]
        private string _totalLbText = string.Empty;

        /// <summary>Last Visit: date, qty, price (when PreviouslyOrdered).</summary>
        [ObservableProperty]
        private string _lastVisitText = string.Empty;

        /// <summary>Per week: value (when PreviouslyOrdered and PerWeek > 0).</summary>
        [ObservableProperty]
        private string _perWeekText = string.Empty;

        [ObservableProperty]
        private bool _showLastVisit;

        [ObservableProperty]
        private bool _showPerWeek;

        [ObservableProperty]
        private string _quantityButtonText = "+";

        [ObservableProperty]
        private bool _isEnabled = true;

        [ObservableProperty]
        private Color _productNameColor = Colors.Black;

        [ObservableProperty]
        private Color _onHandColor = Colors.Black;

        /// <summary>True when line has value and is ready to finalize (green Add button).</summary>
        [ObservableProperty]
        private bool _isReadyStyle;

        /// <summary>True when line has value but not ready (red Add button).</summary>
        [ObservableProperty]
        private bool _isPendingStyle;

        /// <summary>Line type label: "Return" or "Dump" for credit lines; empty for sales (no label shown).</summary>
        [ObservableProperty]
        private string _lineTypeText = string.Empty;

        /// <summary>
        /// The underlying template line (StandartTemplateLine or GroupedTemplateLine). Used by the page ViewModel for add/edit.
        /// </summary>
        public object? Line { get; set; }

        public void RefreshFromLine()
        {
            if (Line is not TemplateLine tl)
                return;

            var p = tl.Product;
            ProductName = p?.Name ?? "";

            // Build display name: ID + size/weight + name (match reference image "002 8-5lb Cotija Rallado FW")
            var idPart = (p?.ProductId ?? 0).ToString("000", CultureInfo.InvariantCulture);
            var sizePart = "";
            if (p != null)
            {
                if (p.SoldByWeight && p.Weight > 0)
                    sizePart = (string.IsNullOrEmpty(p.Package) || p.Package == "1" ? "" : p.Package + "-") + p.Weight.ToString("0.##", CultureInfo.InvariantCulture) + "lb ";
                else if (!string.IsNullOrEmpty(p.Package) && p.Package != "1")
                    sizePart = p.Package + "-";
            }
            ProductDisplayName = $"{idPart} {sizePart}{ProductName}".Trim();

            AmountText = "Amount: " + tl.Amount.ToCustomString();
            TotalQtyDisplay = tl.TotalQtyString ?? "+";
            OnHandText = "OH: " + tl.OH.ToString(CultureInfo.InvariantCulture);
            OnHandColor = tl.OH <= 0 ? Colors.Red : Colors.Black;
            ProductNameColor = tl.IsCredit ? Colors.Orange : Colors.Blue;
            LineTypeText = tl.IsCredit ? (tl.Damaged ? "Dump" : "Return") : "";

            if (tl.Product?.SoldByWeight == true)
                TotalLbText = "Total Lb: " + tl.TotalWeight.ToString("0.##", CultureInfo.InvariantCulture);
            else
                TotalLbText = "";

            ShowLastVisit = tl.PreviouslyOrdered;
            ShowPerWeek = tl.PreviouslyOrdered && tl.PerWeek > 0;
            if (ShowLastVisit)
                LastVisitText = "Last Visit: " + tl.LastVisit.ToShortDateString() + ", " + tl.PreviouslyOrderedQty.ToString(CultureInfo.InvariantCulture) + (Config.HidePriceInTransaction ? "" : ", " + tl.PreviouslyOrderedPrice.ToCustomString());
            else
                LastVisitText = "";
            if (ShowPerWeek)
                PerWeekText = "Per week: " + tl.PerWeek.ToString("0.##", CultureInfo.InvariantCulture);
            else
                PerWeekText = "";

            QuantityButtonText = tl.HasValue ? (tl.TotalQtyString ?? "+") : "+";
            IsReadyStyle = tl.HasValue && tl.ReadyToFinalize;
            IsPendingStyle = tl.HasValue && !tl.ReadyToFinalize;
        }
    }
}
