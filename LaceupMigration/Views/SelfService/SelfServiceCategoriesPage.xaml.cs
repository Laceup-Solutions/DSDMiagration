using LaceupMigration.ViewModels;
using LaceupMigration.ViewModels.SelfService;
using Microsoft.Maui.Controls;
using Category = LaceupMigration.Category;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCategoriesPage : LaceupContentPage
    {
        public SelfServiceCategoriesPage(SelfServiceCategoriesPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            OverrideBaseMenu = true;
            
            var productsView = this.FindByName<CollectionView>("ProductsCollectionView");
            if (productsView != null)
            {
                productsView.SelectionChanged += ProductsCollectionView_SelectionChanged;
            }
        }

        protected override string? GetRouteName() => "selfservice/categories";

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceCategoriesPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }

        private async void ProductsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ProductViewModel item && BindingContext is SelfServiceCategoriesPageViewModel viewModel)
            {
                await viewModel.ProductSelectedAsync(item);
            }

            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is SearchBar searchBar && BindingContext is SelfServiceCategoriesPageViewModel vm)
            {
                vm.SearchQuery = searchBar.Text ?? string.Empty;
            }
        }

        private async void SearchBar_SearchButtonPressed(object sender, EventArgs e)
        {
            if (BindingContext is SelfServiceCategoriesPageViewModel viewModel)
            {
                if (sender is SearchBar searchBar)
                    searchBar.Unfocus();
                await viewModel.HandleProductSearchSubmitAsync();
            }
        }

        private async void Category_Tapped(object sender, EventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is CategoryViewModel viewModel && BindingContext is SelfServiceCategoriesPageViewModel pageVm)
            {
                if (viewModel.Subcategories.Count == 0)
                {
                    await pageVm.CategorySelectedAsync(viewModel);
                }
                else
                {
                    await pageVm.ToggleCategoryExpanded(viewModel);
                }
            }
        }

        private async void Subcategory_Tapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is Category subcategory && BindingContext is SelfServiceCategoriesPageViewModel pageVm)
            {
                var categoryViewModel = new CategoryViewModel
                {
                    Category = subcategory,
                    Name = subcategory.Name
                };
                await pageVm.CategorySelectedAsync(categoryViewModel);
            }
        }

        private void Subcategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is CollectionView cv)
            {
                cv.SelectedItem = null;
            }
        }

        private async void ProductImage_Tapped(object sender, EventArgs e)
        {
            if (sender is Image image && image.BindingContext is ProductViewModel viewModel && BindingContext is SelfServiceCategoriesPageViewModel pageVm)
            {
                await pageVm.ProductImageClickedAsync(viewModel);
            }
        }

        private async void ProductCell_Tapped(object sender, EventArgs e)
        {
            if (sender is Grid grid && grid.BindingContext is ProductViewModel viewModel && BindingContext is SelfServiceCategoriesPageViewModel pageVm)
            {
                await pageVm.ViewProductDetailsAsync(viewModel);
            }
        }
    }
}
