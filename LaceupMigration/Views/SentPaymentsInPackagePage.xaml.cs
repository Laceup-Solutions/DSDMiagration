using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class SentPaymentsInPackagePage : ContentPage, IQueryAttributable
    {
        private readonly SentPaymentsInPackagePageViewModel _viewModel;

        public SentPaymentsInPackagePage(SentPaymentsInPackagePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("packagePath", out var value) && value != null)
            {
                var packagePath = value.ToString() ?? string.Empty;
                Dispatcher.Dispatch(async () => await _viewModel.InitializeAsync(packagePath));
            }
        }
    }
}

