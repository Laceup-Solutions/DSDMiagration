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
            PaymentComponentViewModel? component = null;
            
            if (sender is Button button)
            {
                component = button.BindingContext as PaymentComponentViewModel;
            }
            else if (sender is Frame frame)
            {
                component = frame.BindingContext as PaymentComponentViewModel;
            }
            
            if (component != null)
            {
                await _viewModel.EditPayment(component);
            }
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            if (sender is TapGestureRecognizer recognizer)
            {
                var frame = recognizer.Parent as Frame;
                if (frame != null)
                {
                    var component = frame.BindingContext as PaymentComponentViewModel;
                    if (component != null)
                    {
                        await _viewModel.EditPayment(component);
                    }
                }
            }
        }
    }
}

