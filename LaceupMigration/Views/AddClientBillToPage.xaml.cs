using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class AddClientBillToPage : ContentPage, IQueryAttributable
    {
        private readonly AddClientBillToPageViewModel _viewModel;

        public AddClientBillToPage(AddClientBillToPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.OnNavigatedTo(query);
        }
    }
}

