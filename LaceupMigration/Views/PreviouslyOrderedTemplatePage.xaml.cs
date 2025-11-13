using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class PreviouslyOrderedTemplatePage : ContentPage, IQueryAttributable
    {
        private readonly PreviouslyOrderedTemplatePageViewModel _viewModel;

        public PreviouslyOrderedTemplatePage(PreviouslyOrderedTemplatePageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }
    }
}

