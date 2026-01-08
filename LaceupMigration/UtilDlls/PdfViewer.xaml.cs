using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Maui.ApplicationModel.Communication;
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
    public partial class PdfViewer : IQueryAttributable
    {
        private string _pdfPath = string.Empty;
        private int? _orderId;
        private Order _order;

        public PdfViewer()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            // Use custom menu to prevent base class from adding duplicate MENU button
            UseCustomMenu = true;
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
            // Use unified AdvancedOptionsService instead of duplicating logic
            var advancedOptionsService = Handler?.MauiContext?.Services.GetService<Services.AdvancedOptionsService>();
            if (advancedOptionsService != null)
            {
                await advancedOptionsService.ShowAdvancedOptionsAsync();
            }
            else
            {
                // Fallback if service is not available
                await DisplayAlert("Error", "Advanced options service is not available.", "OK");
            }
        }

        private async Task SendByEmailAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_pdfPath) || !File.Exists(_pdfPath))
                {
                    await DisplayAlert("Error", "PDF file not available.", "OK");
                    return;
                }

                // Handle catalog PDF (no order) - matches Xamarin ProductCatalogPdfViewerActivity
                if (_order == null)
                {
                    try
                    {
                        var emailMessage = new EmailMessage();

                        // Set subject and body if Config.SoutoBottomEmailText is set (matches Xamarin)
                        if (!string.IsNullOrEmpty(Config.SoutoBottomEmailText))
                        {
                            emailMessage.Subject = "Souto Foods Product Catalog";
                            emailMessage.BodyFormat = EmailBodyFormat.Html;
                            emailMessage.Body = $"<html><body>{Config.SoutoBottomEmailText}</body></html>";
                        }
                        else
                        {
                            // Default subject if no custom text
                            emailMessage.Subject = "Product Catalog";
                        }

                        // Add PDF attachment
                        emailMessage.Attachments.Add(new EmailAttachment(_pdfPath));

                        await Email.ComposeAsync(emailMessage);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog($"Error sending catalog email via Email.ComposeAsync: {ex.Message}");
                        Logger.CreateLog(ex);
                        
                        // Fall back to platform-specific method if Email.ComposeAsync fails
                        // This is more reliable and matches how other reports are sent
                        try
                        {
                            Config.helper?.SendReportByEmail(_pdfPath);
                        }
                        catch (Exception fallbackEx)
                        {
                            Logger.CreateLog($"Error sending catalog email via SendReportByEmail: {fallbackEx.Message}");
                            Logger.CreateLog(fallbackEx);
                            await DisplayAlert("Error", $"Unable to send email: {ex.Message}. Please check if an email client is configured.", "OK");
                        }
                    }
                    return;
                }

                // Handle order PDF (existing logic)
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