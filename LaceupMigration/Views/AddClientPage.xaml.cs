using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class AddClientPage : ContentPage, IQueryAttributable
    {
        private readonly AddClientPageViewModel _viewModel;

        public AddClientPage(AddClientPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.OnNavigatedTo(query);
            _viewModel.OnBillToSelected(query);
        }
    }
}

