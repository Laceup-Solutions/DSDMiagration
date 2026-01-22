using LaceupMigration.ViewModels;
using System.Collections.Generic;

namespace LaceupMigration.Views
{
    public partial class GoalDetailsPage : LaceupContentPage, IQueryAttributable
    {
        private readonly GoalDetailsPageViewModel _viewModel;

        public GoalDetailsPage(GoalDetailsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.OnNavigatedTo(query);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.FilterDetails(e.NewTextValue);
        }

        private void MenuButton_Clicked(object sender, EventArgs e)
        {
            _viewModel.ShowFilterPopupCommand.Execute(null);
        }

        private void TimeFrameRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && e.Value)
            {
                if (radioButton.Content?.ToString() == "Monthly")
                {
                    _viewModel.FilterMonthly = true;
                    _viewModel.FilterWeekly = false;
                }
                else if (radioButton.Content?.ToString() == "Weekly")
                {
                    _viewModel.FilterMonthly = false;
                    _viewModel.FilterWeekly = true;
                }
            }
        }

        private void SortByRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is RadioButton radioButton && e.Value)
            {
                var content = radioButton.Content?.ToString();
                _viewModel.SortByNone = content == "None";
                _viewModel.SortByName = content == "Product Name";
                _viewModel.SortByCategory = content == "Category";
                _viewModel.SortByCode = content == "Product Code";
            }
        }

        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            return new List<MenuOption>
            {
                new MenuOption("Send by Email", async () => 
                {
                    // Call the ViewModel method
                    await _viewModel.SendGoalByEmailAsync();
                })
            };
        }
    }
}

