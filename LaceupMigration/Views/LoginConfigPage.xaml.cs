using LaceupMigration.ViewModels;

namespace LaceupMigration
{
	public partial class LoginConfigPage : MainTabContentPage
	{
		public LoginConfigPage(LoginConfigPageViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			var model = BindingContext as LoginConfigPageViewModel;
			model?.OnAppearing();
		}

		private void VisualElement_OnFocused(object? sender, FocusEventArgs e)
		{
			if (sender is Entry entry)
			{
				Dispatcher.Dispatch( () =>
				{
					entry.CursorPosition = 0;
					entry.SelectionLength = entry.Text?.Length ?? 0;
				});
			}
		}
	}
}

