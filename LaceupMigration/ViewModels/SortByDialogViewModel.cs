using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace LaceupMigration.ViewModels
{
    public partial class SortByDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isProductNameSelected;

        [ObservableProperty]
        private bool _isProductCodeSelected;

        [ObservableProperty]
        private bool _isCategorySelected;

        [ObservableProperty]
        private bool _isInStockSelected;

        [ObservableProperty]
        private bool _isQtySelected;

        [ObservableProperty]
        private bool _isDescendingSelected;

        [ObservableProperty]
        private bool _isOrderOfEntrySelected;

        [ObservableProperty]
        private bool _isWarehouseLocationSelected;

        [ObservableProperty]
        private bool _isCategoryThenByCodeSelected;

        [ObservableProperty]
        private bool _isJustOrderedChecked;

        private SortDetails.SortCriteria _selectedCriteria;

        public SortByDialogViewModel()
        {
            _selectedCriteria = SortDetails.SortCriteria.ProductName;
        }

        public void Initialize(SortDetails.SortCriteria currentCriteria, bool justOrdered)
        {
            _selectedCriteria = currentCriteria;
            SetSelectedCriteria(currentCriteria);
            IsJustOrderedChecked = justOrdered;
        }

        private void SetSelectedCriteria(SortDetails.SortCriteria criteria)
        {
            // Uncheck all first
            IsProductNameSelected = false;
            IsProductCodeSelected = false;
            IsCategorySelected = false;
            IsInStockSelected = false;
            IsQtySelected = false;
            IsDescendingSelected = false;
            IsOrderOfEntrySelected = false;
            IsWarehouseLocationSelected = false;
            IsCategoryThenByCodeSelected = false;

            // Check the selected one
            switch (criteria)
            {
                case SortDetails.SortCriteria.ProductName:
                    IsProductNameSelected = true;
                    break;
                case SortDetails.SortCriteria.ProductCode:
                    IsProductCodeSelected = true;
                    break;
                case SortDetails.SortCriteria.Category:
                    IsCategorySelected = true;
                    break;
                case SortDetails.SortCriteria.InStock:
                    IsInStockSelected = true;
                    break;
                case SortDetails.SortCriteria.Qty:
                    IsQtySelected = true;
                    break;
                case SortDetails.SortCriteria.Descending:
                    IsDescendingSelected = true;
                    break;
                case SortDetails.SortCriteria.OrderOfEntry:
                    IsOrderOfEntrySelected = true;
                    break;
                case SortDetails.SortCriteria.WarehouseLocation:
                    IsWarehouseLocationSelected = true;
                    break;
                case SortDetails.SortCriteria.CategoryThenByCode:
                    IsCategoryThenByCodeSelected = true;
                    break;
            }
        }

        partial void OnIsProductNameSelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.ProductName;
        }

        partial void OnIsProductCodeSelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.ProductCode;
        }

        partial void OnIsCategorySelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.Category;
        }

        partial void OnIsInStockSelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.InStock;
        }

        partial void OnIsQtySelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.Qty;
        }

        partial void OnIsDescendingSelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.Descending;
        }

        partial void OnIsOrderOfEntrySelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.OrderOfEntry;
        }

        partial void OnIsWarehouseLocationSelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.WarehouseLocation;
        }

        partial void OnIsCategoryThenByCodeSelectedChanged(bool value)
        {
            if (value) _selectedCriteria = SortDetails.SortCriteria.CategoryThenByCode;
        }

        [RelayCommand]
        private async Task ApplyAsync()
        {
            // Send message to PreviouslyOrderedTemplatePageViewModel
            MessagingCenter.Send(this, "SortCriteriaApplied", new Tuple<SortDetails.SortCriteria, bool>(_selectedCriteria, IsJustOrderedChecked));
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}

