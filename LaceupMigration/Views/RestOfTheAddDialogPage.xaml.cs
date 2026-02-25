using LaceupMigration.ViewModels;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views;

public partial class RestOfTheAddDialogPage : ContentPage
{
    public RestOfTheAddDialogPage(RestOfTheAddDialogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        Loaded += (_, _) =>
        {
            var maxH = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * 0.52;
            if (DialogBorder != null)
                DialogBorder.MaximumHeightRequest = maxH;
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is not RestOfTheAddDialogViewModel vm) return;
        await Task.Delay(100);
        var focusEntry = vm.ShowWeightEntry ? WeightEntry : QtyEntry;
        if (focusEntry != null)
        {
            focusEntry.Focus();
            if (!string.IsNullOrEmpty(focusEntry.Text))
            {
                focusEntry.CursorPosition = 0;
                focusEntry.SelectionLength = focusEntry.Text.Length;
            }
        }
    }
}
