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
                        DataAccess.LoadGoalProgressDetail();
                    }
                });
                
                // Get the goal
                var goal = GoalProgressDTO.List.FirstOrDefault(x => x.Id == _goalId);
                if (goal != null)
                {
                    GoalName = goal.Name ?? "Goal";
                    GoalDescription = $"{goal.Type} Goal - {goal.Criteria}";
                    StartDate = goal.StartDate;
                    EndDate = goal.EndDate;
                    
                    // Load goal details
                    _allDetails.Clear();
                    var goalDetails = GoalDetailDTO.List.Where(x => x.GoalId == _goalId).ToList();
                    
                    foreach (var detail in goalDetails)
                    {
                        var item = new GoalDetailItemViewModel
                        {
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

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allDetails
                : _allDetails.Where(x => 
                    x.ItemName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

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
                    DataAccess.LoadGoalProgressDetail();
                });
                
                // Filter details by date range
                var goalDetails = GoalDetailDTO.List
                    .Where(x => x.GoalId == _goalId)
                    .Where(x => x.Goal != null && x.Goal.StartDate >= StartDate && x.Goal.EndDate <= EndDate)
                    .ToList();
                
                _allDetails.Clear();
                foreach (var detail in goalDetails)
                {
                    var item = new GoalDetailItemViewModel
                    {
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

