using LaceupMigration.ViewModels;
using LaceupMigration.Controls;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views
{
    public partial class GoalFilterPage : ContentPage, IQueryAttributable
    {
        private SelectGoalPageViewModel _parentViewModel;
        private DialogService _dialogService;
        private List<GoalCriteria> _selectedFilters = new();
        private DateTime _startDate = DateTime.MinValue;
        private DateTime _endDate = DateTime.MinValue;
        private bool _showExpired = false;

        private bool _checkboxesInitialized = false;

        public GoalFilterPage(DialogService dialogService)
        {
            InitializeComponent();
            _dialogService = dialogService;
            InitializeCheckboxHandlers();
        }

        private void InitializeCheckboxHandlers()
        {
            if (_checkboxesInitialized) return;
            _checkboxesInitialized = true;

            // Handle "All" checkbox - when checked, uncheck others
            AllCheckBox.CheckedChanged += (s, e) =>
            {
                if (AllCheckBox.IsChecked)
                {
                    RouteCheckBox.IsChecked = false;
                    ProductCheckBox.IsChecked = false;
                    PaymentCheckBox.IsChecked = false;
                    CustomerCheckBox.IsChecked = false;
                    ProductByCustomerCheckBox.IsChecked = false;
                }
            };

            // When any other checkbox is checked, uncheck "All"
            void UncheckAll(object s, CheckedChangedEventArgs e)
            {
                if (e.Value) AllCheckBox.IsChecked = false;
            }

            RouteCheckBox.CheckedChanged += UncheckAll;
            ProductCheckBox.CheckedChanged += UncheckAll;
            PaymentCheckBox.CheckedChanged += UncheckAll;
            CustomerCheckBox.CheckedChanged += UncheckAll;
            ProductByCustomerCheckBox.CheckedChanged += UncheckAll;
        }

        public GoalFilterPage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query != null)
            {
                // Get parent ViewModel from query
                if (query.TryGetValue("parentViewModel", out var vm) && vm is SelectGoalPageViewModel parentVm)
                {
                    _parentViewModel = parentVm;
                }

                // Load current filter state
                if (query.TryGetValue("currentFilter", out var filter) && filter is List<GoalCriteria> currentFilter)
                {
                    _selectedFilters = currentFilter ?? new List<GoalCriteria>();
                }

                if (query.TryGetValue("startDateFilter", out var startDate) && startDate is DateTime sd)
                {
                    _startDate = sd;
            if (_startDate != DateTime.MinValue)
            {
                StartDateButton.Text = _startDate.ToShortDateString();
                StartDateButton.TextColor = Colors.Black;
            }
            else
            {
                StartDateButton.Text = "Select Start Date";
                StartDateButton.TextColor = Colors.Gray;
            }
                }

                if (query.TryGetValue("endDateFilter", out var endDate) && endDate is DateTime ed)
                {
                    _endDate = ed;
            if (_endDate != DateTime.MinValue)
            {
                EndDateButton.Text = _endDate.ToShortDateString();
                EndDateButton.TextColor = Colors.Black;
            }
            else
            {
                EndDateButton.Text = "Select End Date";
                EndDateButton.TextColor = Colors.Gray;
            }
                }

                if (query.TryGetValue("showExpiredGoals", out var showExpired) && showExpired is bool se)
                {
                    _showExpired = se;
                    ShowExpiredCheckBox.IsChecked = _showExpired;
                }

                // Set checkboxes based on current filter
                UpdateCheckboxes();
            }
        }

        private void UpdateCheckboxes()
        {
            if (_selectedFilters.Count == 0)
            {
                AllCheckBox.IsChecked = true;
                RouteCheckBox.IsChecked = false;
                ProductCheckBox.IsChecked = false;
                PaymentCheckBox.IsChecked = false;
                CustomerCheckBox.IsChecked = false;
                ProductByCustomerCheckBox.IsChecked = false;
            }
            else
            {
                AllCheckBox.IsChecked = false;
                RouteCheckBox.IsChecked = _selectedFilters.Contains(GoalCriteria.Route);
                ProductCheckBox.IsChecked = _selectedFilters.Contains(GoalCriteria.Product);
                PaymentCheckBox.IsChecked = _selectedFilters.Contains(GoalCriteria.Payment);
                CustomerCheckBox.IsChecked = _selectedFilters.Contains(GoalCriteria.Customer);
                ProductByCustomerCheckBox.IsChecked = _selectedFilters.Contains(GoalCriteria.ProductsByCustomer);
            }
        }

        private async void StartDateButton_Clicked(object sender, EventArgs e)
        {
            var initialDate = _startDate == DateTime.MinValue ? DateTime.Today : _startDate;
            var date = await _dialogService.ShowDatePickerAsync("Start Date", initialDate, DateTime.MinValue, DateTime.MaxValue);
            
            if (date.HasValue)
            {
                _startDate = date.Value;
                StartDateButton.Text = _startDate.ToShortDateString();
                StartDateButton.TextColor = Colors.Black;
            }
        }

        private async void EndDateButton_Clicked(object sender, EventArgs e)
        {
            var initialDate = _endDate == DateTime.MinValue ? DateTime.Today : _endDate;
            var date = await _dialogService.ShowDatePickerAsync("End Date", initialDate, DateTime.MinValue, DateTime.MaxValue);
            
            if (date.HasValue)
            {
                _endDate = date.Value;
                EndDateButton.Text = _endDate.ToShortDateString();
                EndDateButton.TextColor = Colors.Black;
            }
        }

        private void RemoveFiltersButton_Clicked(object sender, EventArgs e)
        {
            _selectedFilters.Clear();
            _startDate = DateTime.MinValue;
            _endDate = DateTime.MinValue;
            _showExpired = false;

            AllCheckBox.IsChecked = true;
            RouteCheckBox.IsChecked = false;
            ProductCheckBox.IsChecked = false;
            PaymentCheckBox.IsChecked = false;
            CustomerCheckBox.IsChecked = false;
            ProductByCustomerCheckBox.IsChecked = false;
            ShowExpiredCheckBox.IsChecked = false;

            StartDateButton.Text = "Select Start Date";
            StartDateButton.TextColor = Colors.Gray;
            EndDateButton.Text = "Select End Date";
            EndDateButton.TextColor = Colors.Gray;
        }

        private async void ApplyButton_Clicked(object sender, EventArgs e)
        {
            // Collect selected filters
            _selectedFilters.Clear();
            if (!AllCheckBox.IsChecked)
            {
                if (RouteCheckBox.IsChecked) _selectedFilters.Add(GoalCriteria.Route);
                if (ProductCheckBox.IsChecked) _selectedFilters.Add(GoalCriteria.Product);
                if (PaymentCheckBox.IsChecked) _selectedFilters.Add(GoalCriteria.Payment);
                if (CustomerCheckBox.IsChecked) _selectedFilters.Add(GoalCriteria.Customer);
                if (ProductByCustomerCheckBox.IsChecked) _selectedFilters.Add(GoalCriteria.ProductsByCustomer);
            }

            _showExpired = ShowExpiredCheckBox.IsChecked;

            // Apply filters to parent ViewModel
            if (_parentViewModel != null)
            {
                _parentViewModel.ApplyFilters(_selectedFilters, _startDate, _endDate, _showExpired);
            }

            await Shell.Current.GoToAsync("..");
        }

        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
