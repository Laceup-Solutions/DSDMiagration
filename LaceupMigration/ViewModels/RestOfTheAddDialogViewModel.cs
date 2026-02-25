using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LaceupMigration.ViewModels;

/// <summary>ViewModel for the RestOfTheAddDialog XAML page (add/edit line: qty, UoM, price, discount, comments, etc.).</summary>
public partial class RestOfTheAddDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private string _qty = "1";
    [ObservableProperty] private string _weight = "0";
    [ObservableProperty] private string _lot = string.Empty;
    [ObservableProperty] private string _comments = string.Empty;
    [ObservableProperty] private string _price = "0";
    [ObservableProperty] private bool _isFreeItem;
    [ObservableProperty] private bool _useLastSoldPrice;
    [ObservableProperty] private string _discountText = "Discount = $0.00";

    [ObservableProperty] private bool _showQtyEntry = true;
    [ObservableProperty] private bool _showWeightEntry;
    [ObservableProperty] private string _qtyLabel = "Qty:";
    [ObservableProperty] private bool _showUoM;
    [ObservableProperty] private bool _showPrice;
    [ObservableProperty] private bool _showFreeItemOnRow;
    [ObservableProperty] private bool _showFreeItemRow;
    [ObservableProperty] private bool _showPriceLevel;
    [ObservableProperty] private bool _showUseLSP;
    [ObservableProperty] private bool _showDiscountLink;
    [ObservableProperty] private bool _showComments;
    [ObservableProperty] private bool _showLotSection;
    [ObservableProperty] private bool _showLotButton;
    [ObservableProperty] private bool _showLotEntry;
    [ObservableProperty] private bool _showExpiration;

    [ObservableProperty] private bool _isDiscountLinkVisible = true;
    [ObservableProperty] private bool _isPriceEnabled = true;

    /// <summary>True when discount link section is shown and not hidden by Free Item.</summary>
    public bool IsDiscountLinkVisibleAndShown => ShowDiscountLink && IsDiscountLinkVisible;

    public ObservableCollection<UnitOfMeasure> UoMItems { get; } = new();
    public ObservableCollection<string> PriceLevelOptions { get; } = new();
    public List<int> PriceLevelIds { get; } = new();

    private int _selectedUoMIndex = -1;
    private int _selectedPriceLevelIndex;
    private double _currentDiscount;
    private DiscountType _currentDiscountType;
    private UnitOfMeasure? _selectedUoM;
    private int _reasonId;
    private double _lastSoldPrice;
    private readonly List<UnitOfMeasure> _uomFamilyList = new();
    private readonly List<ProductPrice> _productPriceList = new();

    private Product _product = null!;
    private Order _order = null!;
    private OrderDetail? _existingDetail;
    private bool _isCredit;
    private bool _isDamaged;
    private InvoiceDetail? _lastInvoiceDetail;

    private Func<RestOfTheAddDialogResult, Task>? _onCompleteAsync;
    private Func<double, DiscountType, double, double, Task<(double discount, DiscountType type)>>? _showDiscountPopupAsync;

    public int SelectedUoMIndex
    {
        get => _selectedUoMIndex;
        set
        {
            if (_selectedUoMIndex == value) return;
            var prevUoM = _selectedUoM;
            _selectedUoMIndex = value;
            OnPropertyChanged();
            if (value >= 0 && value < _uomFamilyList.Count)
            {
                _selectedUoM = _uomFamilyList[value];
                if (ShowPrice && prevUoM != null && _selectedUoM != null && double.TryParse(Price, out var p))
                {
                    var newPrice = Math.Round(p * (_selectedUoM.Conversion / prevUoM.Conversion), Config.Round);
                    Price = newPrice.ToString("F2");
                }
            }
        }
    }

    public int SelectedPriceLevelIndex
    {
        get => _selectedPriceLevelIndex;
        set
        {
            if (_selectedPriceLevelIndex == value) return;
            _selectedPriceLevelIndex = value;
            OnPropertyChanged();
            if (value > 0 && value <= _productPriceList.Count)
            {
                var selectedPP = _productPriceList[value - 1];
                var conv = _selectedUoM != null ? _selectedUoM.Conversion : 1.0;
                var newPrice = Math.Round(selectedPP.Price * conv, Config.Round);
                Price = newPrice.ToString("F2");
            }
        }
    }

    partial void OnIsFreeItemChanged(bool value)
    {
        IsPriceEnabled = !value;
        if (value) Price = "0.00";
        IsDiscountLinkVisible = !value;
        OnPropertyChanged(nameof(IsDiscountLinkVisibleAndShown));
    }

    partial void OnUseLastSoldPriceChanged(bool value)
    {
        if (value) Price = _lastSoldPrice.ToString("F2");
        IsPriceEnabled = !value;
    }

    public void Initialize(
        Product product,
        Order order,
        OrderDetail? existingDetail,
        bool isCredit,
        bool isDamaged,
        bool isDelivery,
        InvoiceDetail? lastInvoiceDetail,
        float initialQty,
        float initialWeight,
        string initialLot,
        string initialComments,
        double initialPrice,
        UnitOfMeasure? initialUoM,
        bool initialFreeItem,
        bool initialUseLSP,
        int initialPriceLevelSelected,
        double initialDiscount,
        DiscountType initialDiscountType,
        int initialReasonId,
        Func<RestOfTheAddDialogResult, Task> onCompleteAsync,
        Func<double, DiscountType, double, double, Task<(double, DiscountType)>> showDiscountPopupAsync)
    {
        _product = product;
        _order = order;
        _existingDetail = existingDetail;
        _isCredit = isCredit;
        _isDamaged = isDamaged;
        _lastInvoiceDetail = lastInvoiceDetail;
        _onCompleteAsync = onCompleteAsync;
        _showDiscountPopupAsync = showDiscountPopupAsync;
        _reasonId = initialReasonId;
        _currentDiscount = initialDiscount;
        _currentDiscountType = initialDiscountType;
        _lastSoldPrice = lastInvoiceDetail?.Price ?? 0;

        ProductName = product.Name;
        Qty = initialQty.ToString(product.SoldByWeight && !order.AsPresale ? "F2" : "F0");
        Weight = initialWeight.ToString("F2");
        Lot = initialLot;
        Comments = initialComments;
        IsFreeItem = initialFreeItem;
        UseLastSoldPrice = initialUseLSP;
        DiscountText = FormatDiscountText(initialDiscount, initialDiscountType);

        if (!order.AsPresale && product.SoldByWeight)
        {
            ShowQtyEntry = false;
            ShowWeightEntry = true;
            QtyLabel = "Weight:";
        }
        else if (Config.EnterWeightInCredits && product.SoldByWeight && order.AsPresale && isCredit)
        {
            ShowWeightEntry = true;
            Qty = "1";
        }

        var canChangePrice = Config.CanChangePrice(order, product, isCredit);
        ShowPrice = canChangePrice;
        ShowFreeItemOnRow = order.OrderType == OrderType.Order && Config.AllowFreeItems && canChangePrice;
        ShowFreeItemRow = order.OrderType == OrderType.Order && Config.AllowFreeItems && !canChangePrice;
        ShowUseLSP = Config.UseLSP && lastInvoiceDetail != null;
        ShowComments = !Config.HideItemComment || (order.OrderType != OrderType.Order && order.OrderType != OrderType.Credit);
        var showDiscountLink = Config.AllowDiscountPerLine
            && order.Client?.UseDiscountPerLine == true
            && (order.OrderType == OrderType.Order || order.OrderType == OrderType.Credit || order.OrderType == OrderType.Return)
            && !Config.HidePriceInTransaction
            && !initialFreeItem;
        ShowDiscountLink = showDiscountLink;
        IsDiscountLinkVisible = showDiscountLink;
        OnPropertyChanged(nameof(IsDiscountLinkVisibleAndShown));

        if (!order.AsPresale && !isDamaged && (product.UseLot || product.UseLotAsReference))
        {
            ShowLotSection = true;
            ShowLotButton = product.UseLot;
            ShowLotEntry = !product.UseLot;
            ShowExpiration = product.UseLot && Config.UseLotExpiration;
        }

        if (!product.SoldByWeight && !string.IsNullOrEmpty(product.UoMFamily))
        {
            var family = UnitOfMeasure.List.Where(x => x.FamilyId == product.UoMFamily).ToList();
            _uomFamilyList.Clear();
            _uomFamilyList.AddRange(family);
            UoMItems.Clear();
            foreach (var u in family) UoMItems.Add(u);
            if (family.Count > 0)
            {
                ShowUoM = true;
                var idx = initialUoM != null ? family.FindIndex(x => x.Id == initialUoM.Id) : 0;
                if (idx < 0) idx = 0;
                _selectedUoMIndex = idx;
                _selectedUoM = family[idx];
                OnPropertyChanged(nameof(SelectedUoMIndex));
            }
        }
        else
        {
            _selectedUoM = initialUoM;
        }

        var initialDisplayPrice = initialPrice;
        if (existingDetail == null && initialUoM != null && !string.IsNullOrEmpty(product.UoMFamily))
            initialDisplayPrice = Math.Round(initialPrice * initialUoM.Conversion, Config.Round);
        Price = initialDisplayPrice.ToString("F2");

        if (canChangePrice)
        {
            var productPrices = ProductPrice.Pricelist.Where(x => x.ProductId == product.ProductId).ToList();
            if (productPrices.Any())
            {
                PriceLevelOptions.Clear();
                PriceLevelIds.Clear();
                _productPriceList.Clear();
                PriceLevelOptions.Add("Select a Price Level");
                PriceLevelIds.Add(0);
                var conv = _selectedUoM != null ? _selectedUoM.Conversion : 1.0;
                foreach (var pp in productPrices)
                {
                    var pl = PriceLevel.List.FirstOrDefault(x => x.Id == pp.PriceLevelId);
                    if (pl != null)
                    {
                        PriceLevelOptions.Add($"{pl.Name}: {Math.Round(pp.Price * conv, Config.Round).ToCustomString()}");
                        PriceLevelIds.Add(pp.PriceLevelId);
                        _productPriceList.Add(pp);
                    }
                }
                if (PriceLevelOptions.Count > 1)
                {
                    ShowPriceLevel = true;
                    _selectedPriceLevelIndex = initialPriceLevelSelected > 0 ? PriceLevelIds.IndexOf(initialPriceLevelSelected) : 0;
                    if (_selectedPriceLevelIndex <= 0) _selectedPriceLevelIndex = 0;
                    OnPropertyChanged(nameof(SelectedPriceLevelIndex));
                }
            }
        }
    }

    private static string FormatDiscountText(double discount, DiscountType dType)
    {
        if (discount == 0) return "Discount = $0.00";
        if (dType == DiscountType.Percent) return "Discount = " + (discount * 100).ToString("F0") + "%";
        return "Discount = $" + discount.ToString("F2");
    }

    [RelayCommand]
    private async Task DiscountTappedAsync()
    {
        if (_showDiscountPopupAsync == null) return;
        double qtyVal = 1;
        float.TryParse(Qty, out var qtyF);
        qtyVal = qtyF;
        if (_product.SoldByWeight && _order.AsPresale)
            qtyVal *= _product.Weight;
        if (ShowWeightEntry && float.TryParse(Weight, out var w))
            qtyVal = w;
        double priceVal = 0;
        double.TryParse(Price, out priceVal);
        var (newDiscount, newType) = await _showDiscountPopupAsync(_currentDiscount, _currentDiscountType, qtyVal, priceVal);
        _currentDiscount = newDiscount;
        _currentDiscountType = newType;
        DiscountText = FormatDiscountText(newDiscount, newType);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var result = new RestOfTheAddDialogResult();
        float.TryParse(Qty, out var qty);
        result.Qty = qty;
        float.TryParse(Weight, out var weight);
        result.Weight = ShowWeightEntry ? weight : qty;
        result.Lot = Lot ?? string.Empty;
        result.Comments = Comments ?? string.Empty;
        result.SelectedUoM = _selectedUoM;
        if (ShowPrice && double.TryParse(Price, out var p))
            result.Price = p;
        else
            result.Price = Product.GetPriceForProduct(_product, _order, out _, _isCredit, _isDamaged, _selectedUoM);
        result.IsFreeItem = IsFreeItem;
        result.UseLastSoldPrice = UseLastSoldPrice;
        result.PriceLevelSelected = _selectedPriceLevelIndex > 0 && _selectedPriceLevelIndex <= PriceLevelIds.Count ? PriceLevelIds[_selectedPriceLevelIndex] : 0;
        result.ReasonId = _reasonId;
        if (_currentDiscountType == DiscountType.Amount && result.Qty > 0 && _currentDiscount > 0)
            result.Discount = _currentDiscount / result.Qty;
        else
            result.Discount = _currentDiscount;
        result.DiscountType = _currentDiscountType;
        result.Cancelled = false;
        if (_onCompleteAsync != null) await _onCompleteAsync(result);
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        if (_onCompleteAsync != null) await _onCompleteAsync(new RestOfTheAddDialogResult { Cancelled = true });
    }
}
