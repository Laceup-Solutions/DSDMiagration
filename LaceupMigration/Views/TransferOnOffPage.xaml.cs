using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class TransferOnOffPage : ContentPage, IQueryAttributable
    {
        private readonly TransferOnOffPageViewModel _viewModel;

        public TransferOnOffPage(TransferOnOffPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("action", out var value) && value != null)
            {
                var action = value.ToString() ?? "transferOn";
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(action));
            }
        }
    }
}

