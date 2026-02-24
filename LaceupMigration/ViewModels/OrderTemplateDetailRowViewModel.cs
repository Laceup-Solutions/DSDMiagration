using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// One row in the template details list (one OrderDetail for the product).
    /// </summary>
    public partial class OrderTemplateDetailRowViewModel : ObservableObject
    {
        [ObservableProperty] private string _uomText = string.Empty;
        [ObservableProperty] private string _priceText = string.Empty;
        [ObservableProperty] private string _lotText = string.Empty;
        [ObservableProperty] private string _lotExpText = string.Empty;
        [ObservableProperty] private string _commentText = string.Empty;
        [ObservableProperty] private string _qtyButtonText = "0";
        [ObservableProperty] private string _statusText = "Pending";
        [ObservableProperty] private Color _statusColor = Colors.Red;
        [ObservableProperty] private bool _isHighlighted;

        public OrderDetail? Detail { get; set; }
    }
}
