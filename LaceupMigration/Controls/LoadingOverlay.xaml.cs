using Microsoft.Maui.Controls;

namespace LaceupMigration.Controls
{
    public partial class LoadingPopup : ContentView
    {
        public static readonly BindableProperty MessageProperty = BindableProperty.Create(
            nameof(Message), typeof(string), typeof(LoadingPopup), string.Empty);

        public LoadingPopup()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value ?? "");
        }
        
    }
}
