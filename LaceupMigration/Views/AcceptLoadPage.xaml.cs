using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class AcceptLoadPage : ContentPage, IQueryAttributable
    {
        private readonly AcceptLoadPageViewModel _viewModel;

        public AcceptLoadPage(AcceptLoadPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var value) && value != null)
            {
                if (int.TryParse(value.ToString(), out var orderId))
                {
                    Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(orderId));
                }
            }
        }

        private void OnQuantityChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is ViewModels.InventoryLineViewModel lineViewModel)
            {
                if (float.TryParse(e.NewTextValue, out var qty))
                {
                    lineViewModel.Real = qty;
                    lineViewModel.InventoryLine.Real = qty;
                }
            }
        }
    }
}

