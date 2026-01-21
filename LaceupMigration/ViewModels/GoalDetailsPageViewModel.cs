using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class GoalDetailsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<GoalDetailItemViewModel> _allDetails = new();
        private List<GoalDetailDTO> _allGoalDetails = new(); // Store original GoalDetailDTO list for filtering
        private GoalProgressDTO? _goal; // Store the goal object to access Criteria
        private int _goalId;

        [ObservableProperty] private string _goalName = string.Empty;
        [ObservableProperty] private string _goalDescription = string.Empty;
        [ObservableProperty] private double _progressPercentage;
        [ObservableProperty] private string _progressText = string.Empty;
        [ObservableProperty] private Color _progressColor = Colors.Blue;
        [ObservableProperty] private DateTime _startDate = DateTime.Today.AddDays(-30);
        [ObservableProperty] private DateTime _endDate = DateTime.Today;
        [ObservableProperty] private ObservableCollection<GoalDetailItemViewModel> _goalDetails = new();
        [ObservableProperty] private string _searchText = string.Empty;

        public GoalDetailsPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void OnNavigatedTo(IDictionary<string, object> query)
        {
            if (query != null)
            {
                if (query.TryGetValue("goalId", out var goalId) && goalId != null)
                {
                    _goalId = Convert.ToInt32(goalId);
                }
                if (query.TryGetValue("goalName", out var goalName))
                {
                    GoalName = goalName?.ToString() ?? "Goal";
                }
            }
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                IsLoading = true;
                
                await Task.Run(() =>
                {
                    // Load goal progress detail if not already loaded
                    if (GoalDetailDTO.List.Count == 0)
                    {
                        DataProvider.GetGoalProgressDetail();
                    }
                });
                
                // Get the goal
                _goal = GoalProgressDTO.List.FirstOrDefault(x => x.Id == _goalId);
                if (_goal != null)
                {
                    GoalName = _goal.Name ?? "Goal";
                    GoalDescription = $"{_goal.Type} Goal - {_goal.Criteria}";
                    StartDate = _goal.StartDate;
                    EndDate = _goal.EndDate;
                    
                    // Load goal details - store both GoalDetailDTO and ViewModel items
                    _allDetails.Clear();
                    _allGoalDetails.Clear();
                    var goalDetails = GoalDetailDTO.List.Where(x => x.GoalId == _goalId).ToList();
                    _allGoalDetails = goalDetails;
                    
                    foreach (var detail in goalDetails)
                    {
                        var item = new GoalDetailItemViewModel
                        {
                            Detail = detail, // Store reference to original DTO for filtering
                            ItemName = detail.Name ?? (detail.Product != null ? detail.Product.Name : "Item"),
                            Progress = detail.Sold ?? 0,
                            Target = detail.QuantityOrAmount ?? 0,
                            SalesOrder = detail.SalesOrder ?? 0,
                            CreditInvoice = detail.CreditInvoice,
                            ClientName = detail.client?.ClientName ?? "",
                            ProductName = detail.Product?.Name ?? ""
                        };
                        _allDetails.Add(item);
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Goal not found.", "Error", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                UpdateProgress();
                FilterDetails(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading goal details: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateProgress()
        {
            if (_allDetails.Count > 0)
            {
                var totalProgress = _allDetails.Sum(x => x.Progress);
                var totalTarget = _allDetails.Sum(x => x.Target);
                ProgressPercentage = totalTarget > 0 ? totalProgress / totalTarget : 0;
                ProgressText = $"{totalProgress:F0} / {totalTarget:F0} ({ProgressPercentage:P0})";
                
                ProgressColor = ProgressPercentage >= 1.0 ? Colors.Green : 
                               ProgressPercentage >= 0.75 ? Colors.Blue : Colors.Orange;
            }
        }

        public void FilterDetails(string searchText)
        {
            GoalDetails.Clear();

            // Match Xamarin's search logic from GoalsActivity.Refresh() (lines 120-135)
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // No search - show all
                foreach (var detail in _allDetails)
                {
                    GoalDetails.Add(detail);
                }
                return;
            }

            var searchCriteria = searchText.ToLowerInvariant();
            List<GoalDetailDTO> filteredDetails;

            if (_goal != null)
            {
                // Filter based on goal criteria (matching Xamarin logic)
                if (_goal.Criteria == GoalCriteria.Product || _goal.Criteria == GoalCriteria.ProductsByCustomer)
                {
                    // For Product/ProductsByCustomer: search in Product.Name, Upc, Code, Description, Sku
                    var productsIds = Product.Products
                        .Where(x => 
                            (x.Name != null && x.Name.ToLower().Contains(searchCriteria)) ||
                            (x.Upc != null && x.Upc.Contains(searchCriteria)) ||
                            (x.Code != null && x.Code.Contains(searchCriteria)) ||
                            (x.Description != null && x.Description.Contains(searchCriteria)) ||
                            (x.Sku != null && x.Sku.Contains(searchCriteria)))
                        .Select(x => x.ProductId)
                        .ToList();
                    
                    filteredDetails = _allGoalDetails
                        .Where(x => x.ProductId.HasValue && productsIds.Contains(x.ProductId.Value))
                        .ToList();
                }
                else if (_goal.Criteria == GoalCriteria.Customer || _goal.Criteria == GoalCriteria.Route)
                {
                    // For Customer/Route: search in Client.ClientName
                    var clientsId = Client.Clients
                        .Where(x => x.ClientName != null && x.ClientName.ToLower().Contains(searchCriteria))
                        .Select(x => x.ClientId)
                        .ToList();
                    
                    filteredDetails = _allGoalDetails
                        .Where(x => clientsId.Contains(x.ClientId))
                        .ToList();
                }
                else
                {
                    // For other criteria (e.g., Payment): no filtering or show all
                    filteredDetails = _allGoalDetails.ToList();
                }
            }
            else
            {
                // Fallback: simple name search if goal is not available
                filteredDetails = _allGoalDetails
                    .Where(x => 
                        (x.Name != null && x.Name.ToLower().Contains(searchCriteria)) ||
                        (x.Product != null && x.Product.Name != null && x.Product.Name.ToLower().Contains(searchCriteria)))
                    .ToList();
            }

            // Convert filtered GoalDetailDTO to ViewModel items
            var filteredItemIds = filteredDetails.Select(x => x.Id).ToHashSet();
            var filtered = _allDetails.Where(x => x.Detail != null && filteredItemIds.Contains(x.Detail.Id)).ToList();

            foreach (var detail in filtered)
            {
                GoalDetails.Add(detail);
            }
        }

        [RelayCommand]
        private async Task Filter()
        {
            try
            {
                IsLoading = true;
                
                // Reload goal progress detail (it may have date filtering built in)
                await Task.Run(() =>
                {
                    DataProvider.GetGoalProgressDetail();
                });
                
                // Filter details by date range
                var goalDetails = GoalDetailDTO.List
                    .Where(x => x.GoalId == _goalId)
                    .Where(x => x.Goal != null && x.Goal.StartDate >= StartDate && x.Goal.EndDate <= EndDate)
                    .ToList();
                
                _allDetails.Clear();
                _allGoalDetails.Clear();
                _allGoalDetails = goalDetails;
                
                foreach (var detail in goalDetails)
                {
                    var item = new GoalDetailItemViewModel
                    {
                        Detail = detail, // Store reference to original DTO for filtering
                        ItemName = detail.Name ?? (detail.Product != null ? detail.Product.Name : "Item"),
                        Progress = detail.Sold ?? 0,
                        Target = detail.QuantityOrAmount ?? 0,
                        SalesOrder = detail.SalesOrder ?? 0,
                        CreditInvoice = detail.CreditInvoice,
                        ClientName = detail.client?.ClientName ?? "",
                        ProductName = detail.Product?.Name ?? ""
                    };
                    _allDetails.Add(item);
                }
                
                UpdateProgress();
                FilterDetails(SearchText);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error filtering: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [ObservableProperty] private bool _isLoading;
    }

    public partial class GoalDetailItemViewModel : ObservableObject
    {
        public GoalDetailDTO? Detail { get; set; } // Store reference to original DTO for filtering
        
        [ObservableProperty] private string _itemName = string.Empty;
        [ObservableProperty] private double _progress;
        [ObservableProperty] private double _target;
        [ObservableProperty] private double _salesOrder;
        [ObservableProperty] private double _creditInvoice;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _productName = string.Empty;
        
        public double TotalProgress => Progress + SalesOrder + CreditInvoice;
        public double ProgressPercentage => Target > 0 ? (TotalProgress / Target) * 100 : 0;
    }
}

