using LaceupMigration.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LaceupMigration.Views
{
    public partial class PaymentSetValuesPage : ContentPage, IQueryAttributable
    {
        private readonly PaymentSetValuesPageViewModel _viewModel;

        public PaymentSetValuesPage()
        {
            InitializeComponent();
            _viewModel = App.Services.GetRequiredService<PaymentSetValuesPageViewModel>();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _ = _viewModel.OnNavigatedTo(query);
        }

        private async void EditPayment_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is PaymentComponentViewModel component)
            {
                await _viewModel.EditPayment(component);
            }
        }

        private async void DeletePayment_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is PaymentComponentViewModel component)
            {
                await _viewModel.DeletePayment(component);
            }
        }
    }
}

