using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Maui.ApplicationModel;

namespace LaceupMigration.ViewModels
{
    public partial class PaymentSelectClientPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private List<ClientItemViewModel> _allClients = new();
        private Timer? _searchDebounceTimer;
        private const int SearchDebounceMs = 300;

        [ObservableProperty]
        private ObservableCollection<ClientItemViewModel> _clients = new();

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        public PaymentSelectClientPageViewModel(DialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task OnAppearingAsync()
        {
            LoadClients();
        }

        partial void OnSearchQueryChanged(string value)
        {
            // Debounce search
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new Timer(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() => FilterClients());
            }, null, SearchDebounceMs, Timeout.Infinite);
        }

        private void LoadClients()
        {
            _allClients.Clear();
            Clients.Clear();

            // Build master list of clients with open invoices/orders
            var masterList = new List<ClientItemViewModel>();

            // Add clients with open invoices
            foreach (var invoice in Invoice.OpenInvoices)
            {
                var openAmount = invoice.Balance;

                // Check existing payments
                foreach (var payment in InvoicePayment.List)
                {
                    if (!string.IsNullOrEmpty(payment.InvoicesId))
                    {
                        var pIdAsString = invoice.InvoiceId.ToString();
                        if (Config.SavePaymentsByInvoiceNumber)
                            pIdAsString = invoice.InvoiceNumber;

                        foreach (var idAsString in payment.InvoicesId.Split(','))
                        {
                            if (pIdAsString == idAsString)
                            {
                                if (payment.TotalPaid >= openAmount)
                                {
                                    openAmount = 0;
                                    break;
                                }
                                else
                                {
                                    openAmount -= payment.TotalPaid;
                                }
                            }
                        }
                    }
                }

                // Check temporal payments
                foreach (var tempPayment in TemporalInvoicePayment.List)
                {
                    if (tempPayment.invoiceId == invoice.InvoiceId)
                    {
                        if (tempPayment.amountPaid == 0)
                        {
                            openAmount = 0;
                            break;
                        }
                        else
                        {
                            openAmount = tempPayment.amountPaid;
                        }
                    }
                }

                if (openAmount > 0 || (Config.ShowInvoicesCreditsInPayments && openAmount < 0))
                {
                    if (!masterList.Any(x => x.ClientId == invoice.Client.ClientId))
                    {
                        masterList.Add(new ClientItemViewModel
                        {
                            ClientId = invoice.Client.ClientId,
                            ClientName = invoice.Client.ClientName
                        });
                    }
                }
            }

            // Add clients with finished orders
            foreach (var order in Order.Orders.Where(x => x.Finished && !x.Voided).ToList())
            {
                var openAmount = order.OrderTotalCost();

                foreach (var payment in InvoicePayment.List)
                {
                    if (payment.Orders().Any(o => o.OrderId == order.OrderId))
                    {
                        if (payment.TotalPaid >= openAmount)
                        {
                            openAmount = 0;
                            break;
                        }
                        else
                        {
                            openAmount -= payment.TotalPaid;
                        }
                    }
                }

                if (openAmount > 0)
                {
                    if (!masterList.Any(x => x.ClientId == order.Client.ClientId))
                    {
                        masterList.Add(new ClientItemViewModel
                        {
                            ClientId = order.Client.ClientId,
                            ClientName = order.Client.ClientName
                        });
                    }
                }
            }

            // If UseCreditAccount is enabled, show all clients
            if (Config.UseCreditAccount)
            {
                foreach (var client in Client.Clients)
                {
                    if (!masterList.Any(x => x.ClientId == client.ClientId))
                    {
                        masterList.Add(new ClientItemViewModel
                        {
                            ClientId = client.ClientId,
                            ClientName = client.ClientName
                        });
                    }
                }
            }

            _allClients = masterList.OrderBy(x => x.ClientName).ToList();
            FilterClients();
        }

        private void FilterClients()
        {
            if (string.IsNullOrEmpty(SearchQuery))
            {
                Clients.Clear();
                foreach (var client in _allClients)
                {
                    Clients.Add(client);
                }
            }
            else
            {
                var filtered = _allClients.Where(x => 
                    x.ClientName.ToLowerInvariant().Contains(SearchQuery.ToLowerInvariant())).ToList();
                
                Clients.Clear();
                foreach (var client in filtered)
                {
                    Clients.Add(client);
                }
            }
        }

        [RelayCommand]
        public async Task SelectClientAsync(int clientId)
        {
            await Shell.Current.GoToAsync($"selectinvoice?clientId={clientId}&fromPaymentTab=1&fromClientDetails=0");
        }
    }

    public partial class ClientItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _clientId;

        [ObservableProperty]
        private string _clientName = string.Empty;
    }
}

