using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class RouteExpensesPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<RouteExpenseViewModel> _expenses = new();
        [ObservableProperty] private double _totalExpenses;
        private List<RouteExpenseViewModel> _allExpenses = new();

        public RouteExpensesPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                // Load existing expenses from RouteExpenses
                RouteExpenses.LoadExpenses();
                
                Expenses.Clear();
                _allExpenses.Clear();
                
                // Get expense products
                var expenseProducts = Product.Products.Where(x => x.IsExpense).ToList();
                
                // Load existing expenses if available
                if (RouteExpenses.CurrentExpenses != null)
                {
                    foreach (var det in RouteExpenses.CurrentExpenses.Details)
                    {
                        var product = Product.Find(det.ProductId);
                        if (product != null)
                        {
                            var expense = new RouteExpenseViewModel
                            {
                                ProductId = det.ProductId,
                                ProductName = product.Name ?? "Unknown",
                                Amount = det.Amount,
                                Category = "Expense",
                                Date = DateTime.Now
                            };
                            _allExpenses.Add(expense);
                            Expenses.Add(expense);
                        }
                    }
                }
                
                // Add all expense products that don't have entries yet
                foreach (var product in expenseProducts)
                {
                    if (!_allExpenses.Any(x => x.ProductId == product.ProductId))
                    {
                        var expense = new RouteExpenseViewModel
                        {
                            ProductId = product.ProductId,
                            ProductName = product.Name ?? "Unknown",
                            Amount = 0,
                            Category = "Expense",
                            Date = DateTime.Now
                        };
                        _allExpenses.Add(expense);
                        Expenses.Add(expense);
                    }
                }
                
                UpdateTotal();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading expenses: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private void UpdateTotal()
        {
            TotalExpenses = Expenses.Sum(x => x.Amount);
        }

        [RelayCommand]
        private async Task AddExpense()
        {
            // Show product selection for expense products
            var expenseProducts = Product.Products.Where(x => x.IsExpense).ToList();
            
            if (expenseProducts.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No expense products available.", "Info", "OK");
                return;
            }
            
            var productNames = expenseProducts.Select(x => x.Name ?? $"Product {x.ProductId}").ToArray();
            var selectedProductName = await _dialogService.ShowActionSheetAsync("Select Expense Product", "", "Cancel", productNames);
            
            if (string.IsNullOrEmpty(selectedProductName) || selectedProductName == "Cancel")
                return;
            
            var selectedProduct = expenseProducts.FirstOrDefault(x => x.Name == selectedProductName);
            if (selectedProduct == null)
                return;
            
            // Check if already exists
            var existing = Expenses.FirstOrDefault(x => x.ProductId == selectedProduct.ProductId);
            if (existing != null)
            {
                await _dialogService.ShowAlertAsync("This expense product is already in the list. Edit the existing entry.", "Info", "OK");
                return;
            }
            
            var amountText = await _dialogService.ShowPromptAsync("Add Expense", $"Enter amount for {selectedProductName}", "OK", "Cancel", "0", -1, "");
            if (string.IsNullOrWhiteSpace(amountText) || !double.TryParse(amountText, out var amount))
                return;
            
            var expense = new RouteExpenseViewModel
            {
                ProductId = selectedProduct.ProductId,
                ProductName = selectedProductName,
                Amount = amount,
                Category = "Expense",
                Date = DateTime.Now
            };

            Expenses.Add(expense);
            _allExpenses.Add(expense);
            UpdateTotal();
        }
        
        [RelayCommand]
        private async Task EditExpense(RouteExpenseViewModel expense)
        {
            if (expense == null)
                return;
            
            var amountText = await _dialogService.ShowPromptAsync("Edit Expense", $"Enter amount for {expense.ProductName}", "OK", "Cancel", expense.Amount.ToString(), -1, "");
            if (string.IsNullOrWhiteSpace(amountText) || !double.TryParse(amountText, out var amount))
                return;
            
            expense.Amount = amount;
            UpdateTotal();
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                // Save expenses using RouteExpenses class
                var expenses = new RouteExpenses();
                expenses.SessionId = Config.SessionId;
                
                foreach (var expense in Expenses.Where(x => x.Amount > 0))
                {
                    expenses.Details.Add(new RouteExpenseDetail
                    {
                        ProductId = expense.ProductId,
                        Amount = expense.Amount
                    });
                }
                
                RouteExpenses.SaveExpenses(expenses);
                
                await _dialogService.ShowAlertAsync("Expenses saved successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving expenses: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private void DeleteExpense(RouteExpenseViewModel expense)
        {
            if (expense != null)
            {
                Expenses.Remove(expense);
                UpdateTotal();
            }
        }
    }

    public partial class RouteExpenseViewModel : ObservableObject
    {
        [ObservableProperty] private int _productId;
        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private double _amount;
        [ObservableProperty] private string _category = string.Empty;
        [ObservableProperty] private DateTime _date;
        
        public string Description => ProductName;
    }
}

