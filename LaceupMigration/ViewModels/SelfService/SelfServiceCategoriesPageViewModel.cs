using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceCategoriesPageViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order _order;

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<CategoryItemViewModel> _categories = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        private int _currentFilter = 0; // 0 = All, 1 = Previously Ordered

        public SelfServiceCategoriesPageViewModel(IDialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void ApplyQueryAttributes(System.Collections.Generic.IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderIdObj) && int.TryParse(orderIdObj?.ToString(), out var orderId))
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
                if (_order != null)
                {
                    ClientName = _order.Client?.ClientName ?? string.Empty;
                }
            }

            LoadCategories();
        }

        public void OnAppearing()
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            Categories.Clear();

            var categoryList = Category.Categories.ToList();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                categoryList = categoryList.Where(x => x.Name.ToLowerInvariant().Contains(searchLower)).ToList();
            }

            // Apply filter
            if (_currentFilter == 1) // Previously Ordered
            {
                var previouslyOrderedCategoryIds = _order?.Client?.OrderedList
                    .Select(x => x.Last.Product.CategoryId)
                    .Distinct()
                    .ToList() ?? new System.Collections.Generic.List<int>();

                categoryList = categoryList.Where(x => previouslyOrderedCategoryIds.Contains(x.CategoryId)).ToList();
            }

            foreach (var category in categoryList.OrderBy(x => x.Name))
            {
                Categories.Add(new CategoryItemViewModel(category));
            }
        }

        [RelayCommand]
        private async Task Filter()
        {
            var options = new[] { "All", "Previously Ordered" };
            var selected = await _dialogService.ShowActionSheetAsync("Filter Categories", "", "Cancel", options);

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
                return;

            _currentFilter = selected == "Previously Ordered" ? 1 : 0;
            LoadCategories();
        }

        [RelayCommand]
        private async Task SelectCategory(CategoryItemViewModel categoryItem)
        {
            if (categoryItem == null || _order == null)
                return;

            // Reuse ProductCatalogPage (same as PreviouslyOrderedTemplate flow)
            await Shell.Current.GoToAsync($"productcatalog?orderId={_order.OrderId}&clientId={_order.Client.ClientId}&categoryId={categoryItem.Category.CategoryId}&comingFrom=SelfService");
        }

        partial void OnSearchTextChanged(string value)
        {
            LoadCategories();
        }
    }

    public partial class CategoryItemViewModel : ObservableObject
    {
        public Category Category { get; }

        [ObservableProperty]
        private string _categoryName;

        public CategoryItemViewModel(Category category)
        {
            Category = category;
            CategoryName = category.Name;
        }
    }
}

