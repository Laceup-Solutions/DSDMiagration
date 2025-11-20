using Microsoft.Maui;

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
}

public class DialogHelper
{
    public static IDialogService? _dialogService { get; set; }
}