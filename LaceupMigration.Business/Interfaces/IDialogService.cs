using Microsoft.Maui;
using System.Collections.Generic;

namespace LaceupMigration;

public interface IDialogService
{
    Task ShowAlertAsync(string message, string title = "Alert", string acceptText = "OK");
    Task<bool> ShowConfirmationAsync(string title, string message, string acceptText = "Yes", string cancelText = "No");
    Task<string> ShowActionSheetAsync(string title, string message, string cancelText, params string[] buttons);
    Task<string> ShowPromptAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancel", string placeholder = "", int maxLength = -1, string initialValue = "", Keyboard keyboard = null, bool showScanIcon = false, Func<Task<string>> scanAction = null);
    Task<bool> ShowConfirmAsync(string message, string title = "Confirm", string acceptText = "Yes", string cancelText = "No");
    Task<int> ShowSelectionAsync(string title, string[] options);
    /// <summary>Single-choice dialog with radio buttons. Returns selected index or -1 if canceled.</summary>
    Task<int> ShowSingleChoiceDialogAsync(string title, string[] options, int selectedIndex = 0);
    /// <summary>Single-choice dialog with radio buttons and optional subtitle per option. Returns selected index or -1 if canceled.</summary>
    Task<int> ShowSingleChoiceDialogAsync(string title, (string Title, string Subtitle)[] options, int selectedIndex = 0);
    Task<DateTime?> ShowDatePickerAsync(string title, DateTime? initialDate = null, DateTime? minimumDate = null, DateTime? maximumDate = null);
    
    Task ShowLoadingAsync(string message = "Loading...");
    Task HideLoadingAsync();
    Task UpdateLoadingMessageAsync(string message);
    Task<(string qty, string comments, UnitOfMeasure selectedUoM)> ShowAddItemDialogAsync(string productName, Product product, string initialQty = "1", string initialComments = "", UnitOfMeasure initialUoM = null);
    Task<(string qty, UnitOfMeasure selectedUoM)> ShowTransferQtyDialogAsync(string productName, Product product, string initialQty = "1", UnitOfMeasure initialUoM = null, string buttonText = "Add");
    Task<(int? priceLevelId, string price, string comments)> ShowPriceLevelDialogAsync(string productName, Product product, Order order, UnitOfMeasure uom, int currentPriceLevelSelected, string initialPrice = "", string initialComments = "");
    /// <param name="hideCategorySection">When true, hide the Categories section (you're already in a category); only "Show In PDF" is shown and OK uses categoryIdsWhenCategorySectionHidden.</param>
    /// <param name="categoryIdsWhenCategorySectionHidden">When hideCategorySection is true, these category IDs are returned as selectedCategoryIds when user taps OK.</param>
    Task<(List<int> selectedCategoryIds, bool selectAll, bool showPrice, bool showUPC, bool showUoM)?> ShowCatalogFilterDialogAsync(List<Category> categories, bool hideCategorySection = false, IReadOnlyList<int>? categoryIdsWhenCategorySectionHidden = null);
    /// <summary>Full add/edit item dialog (quantity, UoM, price, weight, lot, etc.). Same as PreviouslyOrderedTemplate row tap / quantity button.</summary>
    Task<RestOfTheAddDialogResult> ShowRestOfTheAddDialogAsync(Product product, Order order, OrderDetail existingDetail = null, bool isCredit = false, bool isDamaged = false, bool isDelivery = false);
    /// <summary>Shows a dialog to pick one of the product's UOMs. Returns selected UOM or null if cancelled.</summary>
    Task<UnitOfMeasure?> ShowPickUomForProductAsync(Product product, string title = "Select UOM");
    /// <summary>Shows a single popup for Login into new company: Server Address, Port, Salesman Id. Returns (serverAddress, port, salesmanId) or null if cancelled.</summary>
    Task<(string serverAddress, int port, int salesmanId)?> ShowLoginNewCompanyDialogAsync();
}

/// <summary>Result of ShowRestOfTheAddDialogAsync (add/edit item popup).</summary>
public class RestOfTheAddDialogResult
{
    public float Qty { get; set; }
    public float Weight { get; set; }
    public string Lot { get; set; } = string.Empty;
    public DateTime? LotExpiration { get; set; }
    public string Comments { get; set; } = string.Empty;
    public double Price { get; set; }
    public UnitOfMeasure? SelectedUoM { get; set; }
    public bool IsFreeItem { get; set; }
    public bool UseLastSoldPrice { get; set; }
    public int ReasonId { get; set; }
    public int PriceLevelSelected { get; set; }
    /// <summary>Per-line discount amount (currency) or rate (for Percent). Set when user taps Discount link and applies.</summary>
    public double Discount { get; set; }
    /// <summary>Whether Discount is a percentage (Percent) or fixed amount (Amount).</summary>
    public DiscountType DiscountType { get; set; }
    public bool Cancelled { get; set; }
}

public class DialogHelper
{
    public static IDialogService? _dialogService { get; set; }
}