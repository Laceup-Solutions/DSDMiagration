using LaceupMigration.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Maui.Controls;

namespace LaceupMigration.Views.ItemGroupedTemplate
{
    public partial class NewCreditTemplatePage : LaceupContentPage, IQueryAttributable
    {
        private readonly NewCreditTemplatePageViewModel _viewModel;
        private readonly IScannerService _scannerService;

        public NewCreditTemplatePage(NewCreditTemplatePageViewModel viewModel, IScannerService scannerService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _scannerService = scannerService;
            BindingContext = _viewModel;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateColumnDefinitions();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NewCreditTemplatePageViewModel.ActionButtonsColumnDefinitions) ||
                e.PropertyName == nameof(NewCreditTemplatePageViewModel.ShowAddCredit))
            {
                UpdateColumnDefinitions();
            }
        }

        private void UpdateColumnDefinitions()
        {
            if (ActionButtonsGrid == null || _viewModel == null)
                return;

            var columnDefs = _viewModel.ActionButtonsColumnDefinitions;
            var columns = columnDefs.Split(',');

            ActionButtonsGrid.ColumnDefinitions.Clear();
            foreach (var col in columns)
            {
                var colDef = new ColumnDefinition();
                if (col.Trim() == "*")
                    colDef.Width = new GridLength(1, GridUnitType.Star);
                else if (double.TryParse(col.Trim(), out var value))
                    colDef.Width = new GridLength(value);
                else
                    colDef.Width = new GridLength(1, GridUnitType.Star);
                ActionButtonsGrid.ColumnDefinitions.Add(colDef);
            }

            if (!_viewModel.ShowAddCredit)
            {
                if (ProdButton != null) ProdButton.SetValue(Grid.ColumnProperty, 0);
                if (CatsButton != null) CatsButton.SetValue(Grid.ColumnProperty, 1);
                if (SearchButton != null) SearchButton.SetValue(Grid.ColumnProperty, 2);
                if (SendButton != null) SendButton.SetValue(Grid.ColumnProperty, 3);
            }
            else
            {
                if (ProdButton != null) ProdButton.SetValue(Grid.ColumnProperty, 1);
                if (CatsButton != null) CatsButton.SetValue(Grid.ColumnProperty, 2);
                if (SearchButton != null) SearchButton.SetValue(Grid.ColumnProperty, 3);
                if (SendButton != null) SendButton.SetValue(Grid.ColumnProperty, 4);
            }
        }

        protected override List<MenuOption> GetPageSpecificMenuOptions()
        {
            return _viewModel.BuildMenuOptions();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _viewModel.ApplyQueryAttributes(query);

            var route = "newcredittemplate";
            if (query != null && query.Count > 0)
            {
                var queryParams = query
                    .Where(kvp => kvp.Value != null)
                    .Select(kvp => $"{System.Uri.EscapeDataString(kvp.Key)}={System.Uri.EscapeDataString(kvp.Value.ToString()!)}")
                    .ToArray();
                if (queryParams.Length > 0)
                    route += "?" + string.Join("&", queryParams);
            }
            Helpers.NavigationHelper.SaveNavigationState(route);
        }

        protected override string? GetRouteName() => "newcredittemplate";

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.OnAppearingAsync();
            UpdateColumnDefinitions();
            UpdateMenuToolbarItem();

            if (_scannerService != null && Config.ScannerToUse != 4)
            {
                _scannerService.InitScanner();
                _scannerService.StartScanning();
                _scannerService.OnDataScanned += OnDecodeData;
                _scannerService.OnDataScannedQR += OnDecodeDataQR;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_scannerService != null && Config.ScannerToUse != 4)
            {
                _scannerService.OnDataScanned -= OnDecodeData;
                _scannerService.OnDataScannedQR -= OnDecodeDataQR;
                _scannerService.StopScanning();
            }
        }

        protected override async void GoBack()
        {
            await _viewModel.GoBackAsync();
        }

        public async void OnDecodeData(object sender, string data)
        {
            if (string.IsNullOrEmpty(data) || _viewModel == null) return;
            _scannerService?.SetScannerWorking();
            try
            {
                await _viewModel.HandleScannedBarcodeAsync(data);
            }
            finally
            {
                _scannerService?.ReleaseScanner();
            }
        }

        public async void OnDecodeDataQR(object sender, BarcodeDecoder decoder)
        {
            if (decoder == null || _viewModel == null) return;
            _scannerService?.SetScannerWorking();
            try
            {
                await _viewModel.HandleScannedQRCodeAsync(decoder);
            }
            finally
            {
                _scannerService?.ReleaseScanner();
            }
        }
    }
}
