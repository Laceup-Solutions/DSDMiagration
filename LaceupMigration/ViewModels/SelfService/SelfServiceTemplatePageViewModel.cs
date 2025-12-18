using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceTemplatePageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order _order;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<TemplateItemViewModel> _templateItems = new();

        [ObservableProperty]
        private bool _canEdit = true;

        public SelfServiceTemplatePageViewModel(IDialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderIdObj) && int.TryParse(orderIdObj?.ToString(), out var orderId))
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
                if (_order != null)
                {
                    LoadTemplate();
                }
            }
        }

        public void OnAppearing()
        {
            if (_order != null)
            {
                LoadTemplate();
            }
        }

        private void LoadTemplate()
        {
            if (_order == null) return;

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), disable all modifications (only Print allowed)
            CanEdit = !(!_order.AsPresale && (_order.Finished || _order.Voided));

            ClientName = _order.Client.ClientName;
            _order.Client.EnsurePreviouslyOrdered();

            TemplateItems.Clear();
            if (_order.Client.OrderedList != null)
            {
                foreach (var item in _order.Client.OrderedList.OrderByDescending(x => x.Last.Date).Take(50))
                {
                    if (item.Last?.Product != null)
                    {
                        var viewModel = new TemplateItemViewModel(item.Last);
                        viewModel.IsEnabled = CanEdit;
                        TemplateItems.Add(viewModel);
                    }
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanEdit))]
        private async Task AddItem(TemplateItemViewModel item)
        {
            if (item?.LastDetail == null || _order == null || !CanEdit) return;

            var product = item.LastDetail.Product;
            var existingDetail = _order.Details.FirstOrDefault(x => x.Product.ProductId == product.ProductId);

            if (existingDetail != null)
            {
                existingDetail.Qty += (float)item.LastDetail.Quantity;
            }
            else
            {
                var detail = new OrderDetail(product, 0, _order);
                double expectedPrice = Product.GetPriceForProduct(product, _order, false, false);
                double price = 0;
                if (Offer.ProductHasSpecialPriceForClient(product, _order.Client, out price))
                {
                    detail.Price = price;
                    detail.FromOfferPrice = true;
                }
                else
                {
                    detail.Price = expectedPrice;
                    detail.FromOfferPrice = false;
                }
                detail.ExpectedPrice = expectedPrice;
                detail.UnitOfMeasure = product.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);
                detail.Qty = (float)item.LastDetail.Quantity;
                detail.CalculateOfferDetail();
                _order.AddDetail(detail);
            }

            OrderDetail.UpdateRelated(existingDetail ?? _order.Details.Last(), _order);
            _order.RecalculateDiscounts();
            _order.Save();

            await Shell.Current.GoToAsync($"selfservice/checkout?orderId={_order.OrderId}");
        }
    }

    public partial class TemplateItemViewModel : ObservableObject
    {
        public InvoiceDetail LastDetail { get; }

        [ObservableProperty]
        private bool _isEnabled = true;

        public string ProductName => LastDetail?.Product?.Name ?? string.Empty;
        public string LastOrderedText => LastDetail != null ? $"Last ordered: {LastDetail.Date.ToShortDateString()}" : string.Empty;
        public string PriceText => LastDetail != null ? $"Price: {LastDetail.Price.ToCustomString()}" : string.Empty;

        public TemplateItemViewModel(InvoiceDetail detail)
        {
            LastDetail = detail;
        }
    }
}

