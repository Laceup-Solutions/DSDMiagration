using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Extensions.DependencyInjection;
#if ANDROID
using AndroidX.Core.Content;
using Android.Content;
#elif IOS
using Foundation;
#endif

namespace LaceupMigration.UtilDlls
{
    public partial class PdfViewer : ContentPage, IQueryAttributable
    {
        private string _pdfPath = string.Empty;
        private int? _orderId;
        private Order _order;

        public PdfViewer()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("pdfPath", out var pdfPathValue) && pdfPathValue != null)
            {
                _pdfPath = Uri.UnescapeDataString(pdfPathValue.ToString());
            }

            if (query.TryGetValue("orderId", out var orderIdValue) && int.TryParse(orderIdValue.ToString(), out var orderId))
            {
                _orderId = orderId;
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            isLoading.IsVisible = true;
            www.IsVisible = false;
            ErrorLabel.IsVisible = false;

            if (string.IsNullOrEmpty(_pdfPath))
            {
                ShowError("No PDF source provided.");
                return;
            }

            if (!File.Exists(_pdfPath))
            {
                ShowError("PDF file not found.");
                return;
            }

            await Task.Run(() => 
            { 
                Task.Delay(500).Wait();
            });

            try
            {
                www.Uri = _pdfPath;
                www.IsVisible = true;
                isLoading.IsVisible = false;
            }
            catch (Exception ex)
            {
                ShowError($"Error loading PDF: {ex.Message}");
                Logger.CreateLog(ex);
            }
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
            www.IsVisible = false;
            isLoading.IsVisible = false;
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            var action = await DialogHelper._dialogService.ShowActionSheetAsync("Menu", "Cancel", null, 
                "Print", 
                "Send By Email", 
                "Share",
                "Advanced Options");

            if (string.IsNullOrEmpty(action) || action == "Cancel")
                return;

            switch (action)
            {
                case "Print":
                    if (!string.IsNullOrEmpty(_pdfPath) && File.Exists(_pdfPath))
                    {
                        Config.helper?.PrintPdf(_pdfPath);
                    }
                    break;

                case "Send By Email":
                    await SendByEmailAsync();
                    break;

                case "Share":
                    if (!string.IsNullOrEmpty(_pdfPath) && File.Exists(_pdfPath))
                    {
                        await SharePdfAsync();
                    }
                    break;

                case "Advanced Options":
                    await ShowAdvancedOptionsAsync();
                    break;
            }
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            var options = new List<string>
            {
                "Update settings",
                "Send log file",
                "Export data",
                "Remote control",
                "Setup printer"
            };

            if (Config.GoToMain)
            {
                options.Add("Go to main activity");
            }

            var choice = await DialogHelper._dialogService.ShowActionSheetAsync("Advanced Options", "Cancel", null, options.ToArray());
            if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                return;

            var appService = Handler?.MauiContext?.Services.GetService<Services.ILaceupAppService>();
            
            switch (choice)
            {
                case "Update settings":
                    if (appService != null)
                    {
                        await appService.UpdateSalesmanSettingsAsync();
                        await DisplayAlert("Info", "Settings updated.", "OK");
                    }
                    break;
                case "Send log file":
                    if (appService != null)
                    {
                        await appService.SendLogAsync();
                        await DisplayAlert("Info", "Log sent.", "OK");
                    }
                    break;
                case "Export data":
                    if (appService != null)
                    {
                        await appService.ExportDataAsync();
                        await DisplayAlert("Info", "Data exported.", "OK");
                    }
                    break;
                case "Remote control":
                    if (appService != null)
                    {
                        await appService.RemoteControlAsync();
                    }
                    break;
                case "Setup printer":
                    // Navigate to printer setup page
                    await Shell.Current.GoToAsync("setupprinter");
                    break;
                case "Go to main activity":
                    if (appService != null)
                    {
                        await appService.GoBackToMainAsync();
                    }
                    break;
            }
        }

        private async Task SendByEmailAsync()
        {
            try
            {
                if (_order == null)
                {
                    await DisplayAlert("Error", "Order information not available.", "OK");
                    return;
                }

                // Get email addresses
                var toAddresses = new List<string>();
                
                string clientEmail = null;
                if (_order.Client?.ExtraProperties != null)
                {
                    var emailExtra = _order.Client.ExtraProperties.FirstOrDefault(x => x.Item1.ToUpperInvariant() == "EMAIL");
                    if (emailExtra != null && !string.IsNullOrEmpty(emailExtra.Item2))
                        clientEmail = emailExtra.Item2;
                }

                if (Config.ShowAllEmailsAsDestination)
                {
                    var companyEmail = CompanyInfo.SelectedCompany?.CompanyEmail;
                    var salesman = Salesman.List.FirstOrDefault(x => x.Id == _order.SalesmanId);
                    var salesmanEmail = salesman?.Email;

                    if (!string.IsNullOrEmpty(companyEmail))
                        toAddresses.Add(companyEmail);
                    if (!string.IsNullOrEmpty(salesmanEmail))
                        toAddresses.Add(salesmanEmail);
                    if (!string.IsNullOrEmpty(clientEmail))
                        toAddresses.Add(clientEmail);
                }
                else
                {
                    if (!string.IsNullOrEmpty(clientEmail))
                        toAddresses.Add(clientEmail);
                }

                // Build subject and body
                string orderId = _order.OrderId.ToString();
                if (!string.IsNullOrEmpty(_order.PrintedOrderId))
                    orderId = _order.PrintedOrderId;

                string subject = "";
                string body = "";

                switch (_order.OrderType)
                {
                    case OrderType.Order:
                        if (_order.AsPresale)
                        {
                            subject = _order.IsQuote ? "Quote Attached" : "Sales Order Attached";
                            body = subject;
                        }
                        else
                        {
                            subject = "Sales Invoice Attached";
                            body = $"Sales Invoice Num: {orderId}";
                        }
                        break;
                    case OrderType.Credit:
                        subject = "Credit Invoice Attached";
                        body = $"Credit Invoice Num: {orderId}";
                        break;
                    default:
                        subject = "Invoice Attached";
                        body = $"Invoice Num: {orderId}";
                        break;
                }

                // Send email
                Config.helper?.SendOrderByEmail(_pdfPath, subject, body, toAddresses);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await DisplayAlert("Error", "Error sending email.", "OK");
            }
        }

        private async Task SharePdfAsync()
        {
            try
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Share Order PDF",
                    File = new ShareFile(_pdfPath)
                });
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await DisplayAlert("Error", "Error sharing PDF.", "OK");
            }
        }
    }
}