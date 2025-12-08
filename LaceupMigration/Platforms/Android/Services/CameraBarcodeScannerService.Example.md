# Camera Barcode Scanner Service - Usage Example

## Using in a ViewModel

```csharp
using LaceupMigration.Business.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ProductCatalogPageViewModel : ObservableObject
{
    private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;

    public ProductCatalogPageViewModel(
        ICameraBarcodeScannerService cameraBarcodeScanner,
        // ... other dependencies
    )
    {
        _cameraBarcodeScanner = cameraBarcodeScanner;
    }

    [RelayCommand]
    private async Task ScanBarcodeAsync()
    {
        try
        {
            var barcode = await _cameraBarcodeScanner.ScanBarcodeAsync();
            
            if (!string.IsNullOrEmpty(barcode))
            {
                // Use the scanned barcode
                SearchQuery = barcode;
                Filter();
                
                // Or find product by barcode
                var product = FindProductByBarcode(barcode);
                if (product != null)
                {
                    await AddProductAsync(product);
                }
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync($"Error scanning: {ex.Message}", "Error");
        }
    }
}
```

## Using in XAML (SearchBar with Camera Icon)

```xml
<Grid ColumnDefinitions="*,Auto">
    <SearchBar Grid.Column="0"
               Text="{Binding SearchQuery}"
               Placeholder="Search products..."
               SearchButtonPressed="SearchBar_SearchButtonPressed" />
    <ImageButton Grid.Column="1"
                 Source="{mi:MaterialOutlined Icon=QrCode, IconColor=Black}"
                 WidthRequest="40"
                 HeightRequest="40"
                 BackgroundColor="Transparent"
                 Command="{Binding ScanBarcodeCommand}"
                 VerticalOptions="Center"
                 HorizontalOptions="Center" />
</Grid>
```

## Features

- ✅ Native Android CameraX API
- ✅ ML Kit Barcode Scanning (supports all barcode formats)
- ✅ Automatic permission handling
- ✅ Modal scanner page with cancel option
- ✅ Clean service interface for dependency injection
- ✅ Ready for iOS implementation

