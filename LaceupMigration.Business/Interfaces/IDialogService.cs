namespace LaceupMigration;

public interface IDialogService
{
    Task ShowAlertAsync(string message, string title = "Alert", string acceptText = "OK");
    Task<bool> ShowConfirmationAsync(string title, string message, string acceptText = "Yes", string cancelText = "No");
    Task<string> ShowActionSheetAsync(string title, string message, string cancelText, params string[] buttons);
    Task<string> ShowPromptAsync(string title, string message, string acceptText = "OK", string cancelText = "Cancel", string placeholder = "", int maxLength = -1, string initialValue = "");
    
    Task ShowLoadingAsync(string message = "Loading...");
    Task HideLoadingAsync();
}

public class DialogService
{
    public static IDialogService? _dialogService { get; set; }
}