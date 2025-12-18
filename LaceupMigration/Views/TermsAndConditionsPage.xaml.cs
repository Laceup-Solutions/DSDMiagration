using LaceupMigration.ViewModels;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration
{
	public partial class TermsAndConditionsPage
	{
		private readonly TermsAndConditionsPageViewModel _viewModel;

		public TermsAndConditionsPage(TermsAndConditionsPageViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = _viewModel = viewModel;
		}

		protected override bool OnBackButtonPressed()
		{
			if (!_viewModel.CanNavigateBack)
			{
				MainThread.BeginInvokeOnMainThread(async () =>
				{
					await DisplayAlert("Alert", "You must accept the terms and conditions before proceeding.", "OK");
				});
				return true;
			}

			return base.OnBackButtonPressed();
		}
	}
}

