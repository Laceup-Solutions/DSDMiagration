using LaceupMigration.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class SortByDialogPage : ContentPage, IQueryAttributable
    {
        private readonly SortByDialogViewModel _viewModel;

        public SortByDialogPage(SortByDialogViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            SortDetails.SortCriteria currentCriteria = SortDetails.SortCriteria.ProductName;
            bool justOrdered = false;

            if (query.TryGetValue("currentCriteria", out var criteriaValue) && criteriaValue != null)
            {
                if (int.TryParse(criteriaValue.ToString(), out var criteriaInt))
                {
                    currentCriteria = (SortDetails.SortCriteria)criteriaInt;
                }
            }

            if (query.TryGetValue("justOrdered", out var justOrderedValue) && justOrderedValue != null)
            {
                justOrdered = justOrderedValue.ToString() == "1" || justOrderedValue.ToString().ToLowerInvariant() == "true";
            }

            // No need for callback parameter - we'll use MessagingCenter

            _viewModel.Initialize(currentCriteria, justOrdered);
        }

        private void OnJustOrderedLabelTapped(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.IsJustOrderedChecked = !_viewModel.IsJustOrderedChecked;
            }
        }
    }
}

