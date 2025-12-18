using LaceupMigration.ViewModels;

namespace LaceupMigration
{
	public partial class SplashPage 
	{
		private readonly SplashPageViewModel _viewModel;

		public SplashPage(SplashPageViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = _viewModel = viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			
			// [MIGRATION]: Sign Out fix - if user is signed out, immediately redirect to login
			// This allows navigation to ///Splash to clear stack, then immediately go to login
			// Splash won't be visible because redirect happens immediately
			if (!Config.SignedIn)
			{
				_viewModel.RedirectToLoginAsync();
				return;
			}
			
			_viewModel.InitializeCommand.Execute(null);
		}
	}
}

