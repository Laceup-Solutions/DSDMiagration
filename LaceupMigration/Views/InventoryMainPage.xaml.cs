using LaceupMigration.ViewModels;

namespace LaceupMigration.Views
{
    public partial class InventoryMainPage : IQueryAttributable
    {
        private readonly InventoryMainPageViewModel _viewModel;
        private int? _actionIntent;

        public InventoryMainPage(InventoryMainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Match Xamarin: Intent.Extras.Get(actionIntent) - check for actionIntent parameter
            if (query.TryGetValue("actionIntent", out var actionIntentValue))
            {
                if (int.TryParse(actionIntentValue?.ToString(), out var actionIntent))
                {
                    _actionIntent = actionIntent;
                }
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();

            // Match Xamarin: if actionId > 0, trigger AcceptLoad automatically
            if (_actionIntent.HasValue && _actionIntent.Value > 0)
            {
                _actionIntent = null; // Reset to prevent triggering again
                await _viewModel.AcceptLoadCommand.ExecuteAsync(null);
            }
        }
    }
}

