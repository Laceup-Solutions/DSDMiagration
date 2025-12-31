using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class InvoiceDetailsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private Invoice? _invoice;
        private bool _initialized;

        public ObservableCollection<InvoiceDetailItemViewModel> InvoiceDetails { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private bool _showCompanyName;

        [ObservableProperty]
        private string _companyName = string.Empty;

        [ObservableProperty]
        private string _invoiceNumberText = string.Empty;

        [ObservableProperty]
        private string _datesText = string.Empty;

        [ObservableProperty]
        private string _totalText = string.Empty;

        [ObservableProperty]
        private bool _showTotal = true;

        [ObservableProperty]
        private string _discountText = string.Empty;

        [ObservableProperty]
        private bool _showDiscount;

        [ObservableProperty]
        private string _salesmanName = string.Empty;

        [ObservableProperty]
        private bool _showSalesmanName;

        [ObservableProperty]
        private string _comments = string.Empty;

        [ObservableProperty]
        private bool _hasComments;

        public InvoiceDetailsPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
            ShowTotal = !Config.HidePriceInTransaction;
        }

        public async Task InitializeAsync(int invoiceId)
        {
            if (_initialized && _invoice?.InvoiceId == invoiceId)
                return;

            _invoice = Invoice.OpenInvoices.FirstOrDefault(x => x.InvoiceId == invoiceId);
            if (_invoice == null)
            {
                await _dialogService.ShowAlertAsync("Invoice not found.", "Error");
                return;
            }

            _invoice.Client.EnsureInvoicesAreLoaded();

            LoadInvoiceDetails();
            _initialized = true;
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            LoadInvoiceDetails();
            await Task.CompletedTask;
        }

        private void LoadInvoiceDetails()
        {
            if (_invoice == null)
                return;

            ClientName = $"Customer: {_invoice.Client.ClientName}";

            if (!string.IsNullOrEmpty(_invoice.CompanyName) && CompanyInfo.Companies.Count > 1)
            {
                CompanyName = $"Company: {_invoice.CompanyName}";
                ShowCompanyName = true;
            }

            InvoiceNumberText = _invoice.InvoiceType switch
            {
                1 => $"Credit Number: {_invoice.InvoiceNumber}",
                2 => $"Quote Number: {_invoice.InvoiceNumber}",
                3 => $"Sales Order Number: {_invoice.InvoiceNumber}",
                _ => $"Invoice Number: {_invoice.InvoiceNumber}"
            };

            TotalText = _invoice.InvoiceType switch
            {
                1 => $"Credit Total: {_invoice.Amount.ToCustomString()}",
                2 => $"Quote Total: {_invoice.Amount.ToCustomString()}",
                3 => $"Sales Order Total: {_invoice.Amount.ToCustomString()}",
                _ => $"Invoice Total: {_invoice.Amount.ToCustomString()}"
            };

            DatesText = $"Date: {_invoice.Date.ToShortDateString()} | Due: {_invoice.DueDate.ToShortDateString()}";

            var details = _invoice.Details;
            if (details.Count > 0 && !Config.HidePriceInTransaction)
            {
                var totalByDetails = details.Sum(x => x.Quantity * x.Price);
                var discountAmount = Math.Round(totalByDetails - _invoice.Amount, 2);

                if (Math.Round(totalByDetails, 2) != Math.Round(_invoice.Amount, 2) && discountAmount > 0)
                {
                    DiscountText = $"Discount: {discountAmount.ToCustomString()}";
                    ShowDiscount = true;
                }
            }

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == _invoice.SalesmanId);
            if (salesman != null)
            {
                SalesmanName = $"Salesman: {salesman.Name}";
                ShowSalesmanName = true;
            }

            InvoiceDetails.Clear();
            foreach (var detail in details)
            {
                if (detail.Product.Name == "PRODUCT NOT FOUND")
                    continue;

                string uomText = string.Empty;
                bool hasUoM = false;
                if (detail.UnitOfMeasureId > 0)
                {
                    var uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == detail.UnitOfMeasureId);
                    if (uom != null)
                    {
                        uomText = $"UoM: {uom.Name}";
                        hasUoM = true;
                    }
                }

                InvoiceDetails.Add(new InvoiceDetailItemViewModel
                {
                    ProductName = detail.Product.Name,
                    QuantityText = $"Qty: {detail.Quantity}",
                    PriceText = $"Price: {detail.Price.ToCustomString()}",
                    TotalText = $"Total: {(detail.Quantity * detail.Price).ToCustomString()}",
                    UoMText = uomText,
                    HasUoM = hasUoM,
                    ShowPrice = !Config.HidePriceInTransaction,
                    Notes = detail.Comments,
                    HasNotes = !string.IsNullOrWhiteSpace(detail.Comments)
                });
            }

            // Always set comments text (matches Xamarin: commentsTV.Text = GetString(Resource.String.commentsData) + invoice.Comments;)
            Comments = $"Comments: {_invoice.Comments ?? string.Empty}";
            HasComments = true; // Always show comments label (matches Xamarin behavior)
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            if (_invoice == null)
                return;

            var options = BuildMenuOptions();
            if (options.Count == 0)
                return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.Select(o => o.Title).ToArray());
            if (string.IsNullOrWhiteSpace(choice))
                return;

            var option = options.FirstOrDefault(o => o.Title == choice);
            if (option?.Action != null)
            {
                await option.Action();
            }
        }

        private List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            if (_invoice == null)
                return options;

            // Receive Payment / View Payment
            if (Config.PaymentAvailable && !Config.HidePriceInTransaction)
            {
                var existPayment = InvoicePayment.List.FirstOrDefault(x => 
                    x != null && 
                    string.IsNullOrEmpty(x.OrderId) && 
                    (x.Invoices().FirstOrDefault(y => y.InvoiceId == _invoice.InvoiceId) != null));

                var title = existPayment != null ? "View Payment" : "Receive Payment";
                var enabled = _invoice.Balance > 0;

                if (enabled)
                {
                    options.Add(new MenuOption(title, async () =>
                    {
                        if (existPayment != null)
                        {
                            // View existing payment
                            await Shell.Current.GoToAsync($"paymentsetvalues?paymentId={existPayment.Id}&detailViewPayments=1");
                        }
                        else
                        {
                            // Receive new payment - go directly to payment page with invoice pre-selected
                            var invoiceIdParam = Config.SavePaymentsByInvoiceNumber 
                                ? _invoice.InvoiceNumber 
                                : _invoice.InvoiceId.ToString();
                            await Shell.Current.GoToAsync($"paymentsetvalues?clientId={_invoice.ClientId}&invoiceIds={invoiceIdParam}");
                        }
                    }));
                }
            }

            // Print Copy
            options.Add(new MenuOption("Print Copy", async () =>
            {
                await PrintAsync();
            }));

            // Convert to Sales Order
            if (Config.UseQuote && _invoice.InvoiceType == 2 && _invoice.Details.Count > 0 && 
                !Order.Orders.Any(x => x.FromInvoiceId == _invoice.InvoiceId))
            {
                options.Add(new MenuOption("Convert to Order", async () =>
                {
                    await _dialogService.ShowAlertAsync("Convert to Order functionality is not yet fully implemented.", "Info");
                    // TODO: Implement conversion
                }));

                options.Add(new MenuOption("Convert to Invoice", async () =>
                {
                    await _dialogService.ShowAlertAsync("Convert to Invoice functionality is not yet fully implemented.", "Info");
                    // TODO: Implement conversion
                }));
            }

            // Send by Email
            options.Add(new MenuOption("Send by Email", async () =>
            {
                await SendByEmailAsync();
            }));

            // View Attached Photos
            if (DataAccess.CheckCommunicatorVersion(DataAccess.CommunicatorVersion, "46.2.0"))
            {
                options.Add(new MenuOption("View Attached Photos", async () =>
                {
                    await _dialogService.ShowAlertAsync("View Attached Photos functionality is not yet fully implemented.", "Info");
                    // TODO: Implement image viewing
                }));
            }

            // Get Invoice Details
            if (!(_invoice.Details.Count > 0))
            {
                options.Add(new MenuOption("Get Details", async () =>
                {
                    await _dialogService.ShowAlertAsync("Get Details functionality is not yet fully implemented.", "Info");
                    // TODO: Implement getting invoice details from server
                }));
            }

            // Advanced Options
            options.Add(new MenuOption("Advanced Options", ShowAdvancedOptionsAsync));

            return options;
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }

        private async Task SendByEmailAsync()
        {
            if (_invoice == null)
            {
                await _dialogService.ShowAlertAsync("No invoice to send.", "Alert", "OK");
                return;
            }

            try
            {
                // Use PdfHelper to send invoice by email (matches Xamarin InvoiceDetailsActivity)
                await PdfHelper.SendInvoiceByEmail(_invoice);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }

        private async Task PrintAsync()
        {
            if (_invoice == null)
            {
                await _dialogService.ShowAlertAsync("No invoice to print.", "Alert", "OK");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    for (int i = 0; i < number; i++)
                    {
                        if (!printer.PrintOpenInvoice(_invoice))
                            allWent = false;
                    }

                    if (!allWent)
                        return "Error printing invoice.";
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
    }

    public class InvoiceDetailItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string QuantityText { get; set; } = string.Empty;
        public string PriceText { get; set; } = string.Empty;
        public string TotalText { get; set; } = string.Empty;
        public string UoMText { get; set; } = string.Empty;
        public bool HasUoM { get; set; }
        public bool ShowPrice { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
        public bool HasNotes { get; set; }
    }
}

