using Microsoft.Maui;
using System.Collections.Generic;

namespace LaceupMigration;

public interface IDialogService
{
    Task ShowAlertAsync(string message, string title = "Alert", string acceptText = "OK");
    Task<bool> ShowConfirmationAsync(string title, string message, string acceptText = "Yes", string cancelText = "No");
    Task<string> ShowActionSheetAsync(string title, string message, string cancelText, params string[] buttons);
    Task<string> ShowPromptAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancel", string placeholder = "", int maxLength = -1, string initialValue = "", Keyboard keyboard = null);
    Task<bool> ShowConfirmAsync(string message, string title = "Confirm", string acceptText = "Yes", string cancelText = "No");
    Task<int> ShowSelectionAsync(string title, string[] options);
    Task<DateTime?> ShowDatePickerAsync(string title, DateTime? initialDate = null, DateTime? minimumDate = null, DateTime? maximumDate = null);
    
    Task ShowLoadingAsync(string message = "Loading...");
    Task HideLoadingAsync();
    Task UpdateLoadingMessageAsync(string message);
    Task<(string qty, string comments, UnitOfMeasure selectedUoM)> ShowAddItemDialogAsync(string productName, Product product, string initialQty = "1", string initialComments = "", UnitOfMeasure initialUoM = null);
    Task<(string qty, UnitOfMeasure selectedUoM)> ShowTransferQtyDialogAsync(string productName, Product product, string initialQty = "1", UnitOfMeasure initialUoM = null, string buttonText = "Add");
    Task<(int? priceLevelId, string price, string comments)> ShowPriceLevelDialogAsync(string productName, Product product, Order order, UnitOfMeasure uom, int currentPriceLevelSelected, string initialPrice = "", string initialComments = "");
    Task<(List<int> selectedCategoryIds, bool selectAll, bool showPrice, bool showUPC, bool showUoM)?> ShowCatalogFilterDialogAsync(List<Category> categories);
}

public class DialogHelper
{
    public static IDialogService? _dialogService { get; set; }
}